using System;
using System.IO;
using System.IO.Pipes;

class PipeMessenger
{
    public static void Main()
    {
        var pipeName = "test_pipe";

        using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
        {
            Console.WriteLine("Łączenie z serwerem pipe...");
            pipeClient.Connect();
            Console.WriteLine("Połączono!");

            using (var sr = new StreamReader(pipeClient))
            using (var sw = new StreamWriter(pipeClient) { AutoFlush = true })
            {
                string message = "Pozdrowienia z C#!";
                sw.WriteLine(message);
                Console.WriteLine("Wysłano: " + message);

                string response = sr.ReadLine();
                Console.WriteLine("Otrzymano odpowiedź: " + response);
            }
        }

        Console.WriteLine("Zakończono.");
    }
}
