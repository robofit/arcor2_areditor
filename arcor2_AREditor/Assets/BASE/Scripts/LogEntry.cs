using System;
using UnityEngine;

public class LogEntry
{
    private readonly string logType;
    private readonly string logMessage, stackTrace;
    private readonly DateTime timeStamp;
	public LogEntry(string logType, string logMessage, string stackTrace)
	{
        this.logType = logType;
        this.logMessage = logMessage;
        this.stackTrace = stackTrace;
        timeStamp = DateTime.Now;
    }

    public string LogType => logType;

    public string LogMessage => logMessage;

    public string StackTrace => stackTrace;

    public DateTime TimeStamp => timeStamp;

    public override string ToString() {
        return "Timestamp: " + timeStamp.ToString() +
               "\nType: " + LogType.ToString() +
               "\nMessage: " + logMessage +
               "\nStacktrace: " + stackTrace;
    }
}
