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