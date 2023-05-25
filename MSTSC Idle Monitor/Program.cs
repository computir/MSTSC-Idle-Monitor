using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using System.ServiceProcess;
using System.Linq;
using System.Runtime.Versioning;
using System.Diagnostics;

namespace MSTSC_Monitor
{
    static class Program
    {
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MSTSCMonitorService(args)
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                // Running on non-Windows platform. Exiting...
            }
        }
    }

    public partial class MSTSCMonitorService : ServiceBase
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _mainTask;  // Declare _mainTask as nullable

        private static int idleTimeInMinutes;  // idle time
        private static TimeSpan checkingDelay; // checking delay
        private static string dnsQueryUrl = "idletime.corp"; // initialize here

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public MSTSCMonitorService(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ServiceName = "MSTSCMonitorService";
            }
            idleTimeInMinutes = 40;
            checkingDelay = TimeSpan.FromMinutes(5);
            dnsQueryUrl = "idletime.corp";

            foreach (var arg in args)
            {
                var argParts = arg.Split('=');
                if (argParts.Length == 2)
                {
                    switch (argParts[0])
                    {
                        case "--idleTimeInMinutes":
                            if (int.TryParse(argParts[1], out int idleTime))
                            {
                                idleTimeInMinutes = idleTime;
                            }
                            break;
                        case "--checkingDelay":
                            if (double.TryParse(argParts[1], out double delay))
                            {
                                checkingDelay = TimeSpan.FromMinutes(delay);
                            }
                            break;
                        case "--dnsQueryUrl":
                            dnsQueryUrl = argParts[1];
                            break;
                    }
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _mainTask = Main(_cancellationTokenSource.Token);
        }

        protected override void OnStop()
        {
            _cancellationTokenSource?.Cancel();
            _mainTask?.Wait();
        }

        private async Task Main(CancellationToken cancellationToken)
        {
            await GetIdleTimeFromDns();
            await IdleTimeCheck(cancellationToken);
        }

        private static async Task GetIdleTimeFromDns()
        {
            try
            {
                var client = new LookupClient();
                var result = await client.QueryAsync(dnsQueryUrl, QueryType.TXT);
                var txtRecords = result.Answers.TxtRecords();
                if (txtRecords.Any())
                {
                    var firstTxtRecord = txtRecords.FirstOrDefault();
                    if (firstTxtRecord?.Text.Any() == true)
                    {
                        var txtRecord = firstTxtRecord.Text.FirstOrDefault();

                        if (txtRecord != null && int.TryParse(txtRecord, out int tempIdleTime)
                            && tempIdleTime >= 1 && tempIdleTime <= 600)
                        {
                            idleTimeInMinutes = tempIdleTime;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error Getting Idle Time, Using default idle time of {idleTimeInMinutes} minutes...
            }
        }

        private static async Task IdleTimeCheck(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await GetIdleTimeFromDns();

                // Now Waiting for {idleTimeInMinutes} minute(s) of idle time, next check will be in {checkingDelay.TotalMinutes} minutes

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(checkingDelay, cancellationToken);

                    LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                    lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
                    GetLastInputInfo(ref lastInputInfo);
                    var idleTime = (Environment.TickCount - lastInputInfo.dwTime) / 1000 / 60;

                    // System has been idle for {idleTime} minutes

                    if (idleTime >= idleTimeInMinutes)
                    {
                        var mstscProcesses = Process.GetProcessesByName("mstsc");
                        foreach (var process in mstscProcesses)
                        {
                            try
                            {
                                process.Kill();
                                // Killed process: {process.Id}
                            }
                            catch (Exception)
                            {
                                // Failed to kill process: {process.Id}...
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}



