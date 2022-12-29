namespace LogLib;

public enum LogType
{
    Info,
    Warning,
    Error,
    Fatal,
}

public class LogMessage
{
    public string Message { get; }
    public DateTime Time { get; }
    public LogType Type { get; }

    /// <summary>
    /// Defines all the values for the current LogMessage instance
    /// </summary>
    /// <param name="message">Message object to be converted to a string. If the value is null an empty string will be used</param>
    /// <param name="type">Message type to be added to the log message if is set to anything other then "Info"</param>
    public LogMessage(string? message, LogType type)
    {
        Message = message == null ? "" : message;
        Time = DateTime.Now;
        Type = type;
    }

    public override string ToString()
    {
        if (Type == LogType.Info)
            return $"[{Time}] {Message}";
        else
            return $"[{Type.ToString().ToUpper()}][{Time}] {Message}";
    }
}