using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LogLib.Scripts;

public interface ILogger
{
    public void Log(object message);
    public void Log(object message, LogType type);
}

public class Logger : ILogger, IDisposable
{
    public LoggerProperties Properties { get; }
    public string LogFilePath { get; private set; }
    public bool Active { get; private set; } = false;

    StreamWriter stream;
    SerialPort port;

    Queue<LogMessage> messages = new Queue<LogMessage>();

    public Logger(LoggerProperties properties)
    {
        Properties = properties;

        if (Properties.LogToFile && !Directory.Exists(Properties.LogFileDirectoryPath))
            throw new Exception($"Log directory not found at path: {Properties.LogFileDirectoryPath}");

        try
        {
            if (Properties.LogToFile)
            {
                LogFilePath = $"";
                stream = new StreamWriter(LogFilePath, true, Properties.LogFileEncoding);
            }

            if (Properties.LogToSerial)
                port = new SerialPort(Properties.SerialPortName);
        }
        catch (System.Exception ex)
        {
            throw new Exception("Field to initialize one of the Logger's outputs", ex);
        }

        Active = true;
        Task.Factory.StartNew(Run);
    }

    async Task Run()
    {
        while (Active)
        {
            await WriteToLog();
            await Task.Delay(1);
        }

        //Write any remaining log messages if there are any
        await WriteToLog();
    }

    async Task WriteToLog()
    {
        
    }

    public void Log(object message)
    { 

    }

    public void Log(object message, LogType type)
    { 

    }

    public void Dispose()
    {
        Active = false;
    }
}