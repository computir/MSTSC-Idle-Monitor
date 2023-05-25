# MSTSC Monitor Service

This is a .NET console application that can be implemented as a service to monitor the idle time of the system and kill any running MSTSC (Microsoft Remote Desktop) processes if the idle time exceeds a specified threshold.

## Prerequisites

Before running the application, make sure you have the following prerequisites installed on your system:

- .NET Framework (compatible with the version of the application)

## Installation

To install and run the MSTSC Monitor Service, follow these steps:

1. Clone the repository or download the source code files.

2. Build the application using a compatible .NET build tool or integrated development environment (IDE).

3. Once the application is built, you can run it from the command line or implement it as a service.

## Command Line Arguments

The MSTSC Monitor Service accepts the following command line arguments:

- `--idleTimeInMinutes=<value>` (optional): Sets the idle time threshold in minutes. If the system's idle time exceeds this threshold, MSTSC processes will be killed. The default value is 40 minutes.

- `--checkingDelay=<value>` (optional): Sets the delay between idle time checks in minutes. The default value is 5 minutes.

- `--dnsQueryUrl=<value>` (optional): Sets the DNS query URL to retrieve the idle time from. The default value is "idletime.corp".

### Example Usage

Here are a few examples of how to run the MSTSC Monitor Service with different command line arguments:

1. Run the service with default settings:
MyService.exe

2. Set the idle time threshold to 60 minutes and the checking delay to 10 minutes:
MyService.exe --idleTimeInMinutes=60 --checkingDelay=10

3. Change the DNS query URL to "idletime.example.com" and set the idle time threshold to 30 minutes:
MyService.exe --dnsQueryUrl=idletime.example.com --idleTimeInMinutes=30

## Implementing as a Service

To implement the MSTSC Monitor Service as a Windows service, follow these steps:

1. Open the command prompt or PowerShell as an administrator.

2. Navigate to the directory where the compiled `MyService.exe` file is located.

3. Install the service using the `sc` command:
sc create MSTSCMonitorService binPath= "<path_to_MyService.exe>"

Replace `<path_to_MyService.exe>` with the full path to the `MyService.exe` file.

4. Start the service:
sc start MSTSCMonitorService


The service will now be running in the background and monitoring the idle time.

## License

This project is licensed under the [MIT License](LICENSE).

## Troubleshooting

If you encounter any issues or errors while running the MSTSC Monitor Service, please follow these steps:

1. Ensure that you have the necessary permissions to run services on your system.

2. Make sure the required dependencies are installed and properly configured.

3. Check the event logs or console output for any error messages or relevant information.

4. If the issue persists, please open an issue in the project's repository for further assistance.

## Contributing

Contributions to the MSTSC Monitor Service are welcome! If you find any bugs, have feature requests, or want to contribute improvements, please submit a pull request.

## Contact

For additional information or inquiries, please contact the project maintainer
