using System;
using System.IO;
using System.IO.Pipes;

class Program
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
                string message = "";
                string response = "";

                do
                {
                    message = Console.ReadLine();
                    sw.WriteLine(message);
                    Console.WriteLine("Wysłano: " + message);

                    response = sr.ReadLine();
                    Console.WriteLine("Otrzymano odpowiedź: " + response);
                } while (!message.Equals("exit"));
            }
        }

        Console.WriteLine("Zakończono.");
    }
}
