using System.Text;
namespace LogLib;

public class LoggerProperties
{
    public bool LogToFile { get; } = false;
    public string LogFileDirectoryPath { get; }
    public Encoding LogFileEncoding { get; set; } = Encoding.UTF8;

    public bool LogToSerial { get; } = false;
    public string SerialPortName { get; }

    public bool LogToConsole { get; set; } = true;

    public bool IncludeTime { get; set; } = true;

    /// <summary>
    /// Initializes the properties to be used by a Logger
    /// </summary>
    /// <param name="logFileDirectoryPath">The directory where the log file should be created. If none is specified the Logger won't log to a file</param>
    /// <param name="serialPortName">The serial port to output the log messages to. If none is specified the Logger wont log to any serial port</param>
    public LoggerProperties(string logFileDirectoryPath = "", string serialPortName = "")
    {
        LogFileDirectoryPath = "";
        if (!string.IsNullOrWhiteSpace(logFileDirectoryPath))
        {
            LogToFile = true;
            LogFileDirectoryPath = logFileDirectoryPath;
        }

        SerialPortName = "";
        if (!string.IsNullOrWhiteSpace(serialPortName))
        {
            LogToSerial = true;
            SerialPortName = serialPortName;
        }
    }
}