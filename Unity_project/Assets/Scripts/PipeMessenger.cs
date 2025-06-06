using System;
using System.IO;
using System.IO.Pipes;

public static class PipeMessenger
{
    static string pipeName = "test_pipe";

    public static string SendMessage(string s)
    {
        try
        {
            using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
            {
                Console.WriteLine("£¹czenie z serwerem pipe...");
                pipeClient.Connect(1000);
                Console.WriteLine("Po³¹czono!");

                using (var sr = new StreamReader(pipeClient))
                using (var sw = new StreamWriter(pipeClient) { AutoFlush = true })
                {
                    sw.WriteLine(s);
                    Console.WriteLine("Wys³ano: " + s);

                    string response = sr.ReadLine();
                    return response;
                }
            }
        }
        catch (Exception e)
        {
            return $"PipeManager error: {e.Message}";
        }
    }

    public static string EncodeMessage(PipeMessage msg)
    {
        return GetActionCode(msg.action);
    }

    public static PipeMessage DecodeMessage(string msg)
    {
        return new PipeMessage(Action.Message, "topor", 1, 30, "Prosze, milego uzytkowania.");
    }

    public static string GetActionCode(Action a)
    {
        switch (a)
        {
            case Action.Message:
                return "0001";
            case Action.Buy:
                return "0002";
        }
        return "";
    }
}