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
}