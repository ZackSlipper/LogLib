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