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

    //Returns the current DateTime in a string formatted to be usable in a file name
    string TimeString 
    { 
        get
        {
            DateTime now = DateTime.Now;
            return $"{now.Year.ToString("0000")}{now.Month.ToString("00")}{now.Day.ToString("00")}_{now.Hour.ToString("00")}{now.Minute.ToString("00")}{now.Second.ToString("00")}";
        }
    }

    StreamWriter? fileStream;
    SerialPort? serialPort;
    Task runTask;

    Queue<LogMessage> messages = new Queue<LogMessage>();

    public Logger(LoggerProperties properties)
    {
        Properties = properties;
        LogFilePath = "";

        if (Properties.LogToFile && !Directory.Exists(Properties.LogFileDirectoryPath))
            throw new Exception($"Log directory not found at path: {Properties.LogFileDirectoryPath}");

        try
        {
            if (Properties.LogToFile)
            {
                LogFilePath = $"{Properties.LogFileDirectoryPath}/{TimeString}.log";
                fileStream = new StreamWriter(LogFilePath, true, Properties.LogFileEncoding);
            }

            if (Properties.LogToSerial)
                serialPort = new SerialPort(Properties.SerialPortName);
        }
        catch (System.Exception ex)
        {
            throw new Exception("Field to initialize one of the Logger's outputs", ex);
        }

        Active = true;
        runTask = Task.Factory.StartNew(Run);
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
        LogMessage message;
        for (int i = 0; i < messages.Count; i++)
        {
            message = messages.Dequeue();
            if (Properties.LogToFile)
            {
                try
                {
                    if (fileStream != null)
                        await fileStream.WriteLineAsync(message.ToString());
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Filed to write to file: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }

            if (Properties.LogToSerial)
            { 
                try
                {
                    serialPort?.WriteLine(message.ToString());
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Failed to write to serial port: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }

            if (Properties.LogToConsole)
                Console.WriteLine(message.ToString());
        }
    }

    public void Log(object message)
    { 
        if (!Active)
            throw new Exception("Logger has been disposed");

        if (message != null)
            messages.Enqueue(new LogMessage(message.ToString(), LogType.Info));
    }

    public void Log(object message, LogType type)
    {
        if (!Active)
            throw new Exception("Logger has been disposed");

        if (message != null)
            messages.Enqueue(new LogMessage(message.ToString(), type));
    }

    public void Dispose()
    {
        Active = false;

        //WaitFor the Run task to finish
        runTask.GetAwaiter().GetResult();

        if (fileStream != null)
        {
            try
            {
                fileStream.Close();
                fileStream.Dispose();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error closing file stream: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        if (serialPort != null)
        {
            try
            {
                serialPort.Close();
                serialPort.Dispose();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error closing serial port: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}