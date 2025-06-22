using System;
using System.IO;
using UnityEngine;


public class LogManager
{
    private static readonly string LogFilePath = "./log_unity.txt";

    public static void Log(string sender, LogType logType, string msg)
    {
        // construct message
        string message = $"[{sender}] | {msg.Replace("\n", "").Replace("\r", "")}";

        // print message to console
        switch (logType)
        {
            case LogType.LOG:
                Debug.Log(message);
                break;

            case LogType.WARNING:
                Debug.LogWarning(message);
                break;

            case LogType.ERROR:
                Debug.LogError(message);
                break;
        }

        // save message to log
        using (StreamWriter wr = new(LogFilePath, true))
        {
            wr.WriteLine(
                "[" + DateTime.Now.ToString("HH:mm:ss") + "] " +
                message
            );
        }
    }

    public static void InitLog()
    {
        string message = "";
        message += "\n---------------------------------------\n";
        message += $"GAME STARTED {DateTime.Now:dd/mm/yyyy} {DateTime.Now:HH:mm:ss}";
        message += "\n---------------------------------------";

        using (StreamWriter wr = new(LogFilePath, true))
        {
            wr.WriteLine(message);
        }
    }
}
