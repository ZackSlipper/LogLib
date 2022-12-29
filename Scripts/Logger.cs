using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LogLib;

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
    Task? runTask;

    Queue<LogMessage> messages = new Queue<LogMessage>();

    /// <summary>
    /// Initializes the Logger and opens the log file stream or serial port connection if specified in the given properties
    /// </summary>
    /// <param name="properties">Logger properties necessary for opening the serial connection or writing to the log file</param>
    /// <exception cref="Exception">An exception gets thrown in case of invalid property values or failure to open a serial port connection or create a log file stream</exception>
    public Logger(LoggerProperties properties)
    {
        Properties = properties;
        LogFilePath = "";

        //Check if the log file directory provided in the properties exists if necessary
        if (Properties.LogToFile && !Directory.Exists(Properties.LogFileDirectoryPath))
            throw new Exception($"Log directory not found at path: {Properties.LogFileDirectoryPath}");

        //If specified attempt to open the log file stream and/or serial port
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

        //Starts the run task that runs for the lifetime of the logger
        Active = true;
        runTask = Task.Factory.StartNew(Run);

        //In case the application gets terminated dispose of the Logger before closing
        AppDomain.CurrentDomain.ProcessExit += (obj, e) => Dispose();
    }

    /// <summary>
    /// Writes any queued log messages every millisecond
    /// </summary>
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

    /// <summary>
    /// If there are any writes log messages to any outputs allowed by the properties
    /// </summary>
    async Task WriteToLog()
    {
        if (messages.Count == 0)
            return;

        string message;
        for (int i = 0; i < messages.Count; i++)
        {
            //Dequeues the oldest message from the message queue
            message = messages.Dequeue().ToString();

            if (Properties.LogToFile)
            {
                //Attempts to write the message to the log file
                try
                {
                    if (fileStream != null)
                        await fileStream.WriteLineAsync(message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Filed to write to file: {ex.Message}");
                }
            }

            if (Properties.LogToSerial)
            {
                //Attempts to write the message to serial port
                try
                {
                    serialPort?.WriteLine(message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Failed to write to serial port: {ex.Message}");
                }
            }

            if (Properties.LogToConsole)
                Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Adds a message to be written to the log with the "Info" log type
    /// </summary>
    /// <param name="message">Message object to be converted to a string</param>
    /// <exception cref="Exception">The exception gets thrown if the Logger has already been disposed and is inactive</exception>
    public void Log(object message)
    { 
        if (!Active)
            throw new Exception("Logger has been disposed");

        if (message != null)
            messages.Enqueue(new LogMessage(message.ToString(), LogType.Info));
    }

    /// <summary>
    /// Adds a message to be written to the log
    /// </summary>
    /// <param name="message">Message object to be converted to a string</param>
    /// <param name="type">Message type</param>
    /// <exception cref="Exception">The exception gets thrown if the Logger has already been disposed and is inactive</exception>
    public void Log(object message, LogType type)
    {
        if (!Active)
            throw new Exception("Logger has been disposed");

        if (message != null)
            messages.Enqueue(new LogMessage(message.ToString(), type));
    }

    /// <summary>
    /// Stops all log write operations and disposes of any used resources
    /// </summary>
    public void Dispose()
    {
        //Sets active to false to message the run task to stop
        Active = false;

        //WaitFor the Run task to finish if it exists
        if (runTask != null)
        {
            runTask?.GetAwaiter().GetResult();
            runTask = null;
        }

        //If the file stream exist an we try finishing writing to it and closing and disposing of it
        if (fileStream != null)
        {
            try
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error closing file stream: {ex.Message}");
            }

            fileStream = null;
        }

        //If a serial port connection exist an we try closing and disposing of it
        if (serialPort != null)
        {
            try
            {
                serialPort.Close();
                serialPort.Dispose();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error closing serial port: {ex.Message}");
            }

            serialPort = null;
        }
    }
}