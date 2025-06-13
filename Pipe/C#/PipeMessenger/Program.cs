using System;
using System.IO;
using System.IO.Pipes;

class Program
{
    public static void Main()
    {
        string pipeWriteName = @"\\.\pipe\write"; // client → server
        string pipeReadName = @"\\.\pipe\read";   // server → client

        using (var writeStream = new NamedPipeClientStream(".", "write", PipeDirection.Out))
        using (var readStream = new NamedPipeClientStream(".", "read", PipeDirection.In))
        {
            Console.WriteLine("Connecting to pipes...");

            writeStream.Connect();
            Console.WriteLine("Connected to write pipe.");

            readStream.Connect();
            Console.WriteLine("Connected to read pipe.");

            using (var sw = new StreamWriter(writeStream) { AutoFlush = true })
            using (var sr = new StreamReader(readStream))
            {
                string message;
                string response;

                do
                {
                    Console.Write("> ");
                    message = Console.ReadLine();
                    sw.WriteLine(message);
                    Console.WriteLine("[Sent] " + message);

                    response = sr.ReadLine();
                    Console.WriteLine("[Received] " + response);
                } while (!message.Equals("exit", StringComparison.OrdinalIgnoreCase));
            }
        }

        Console.WriteLine("Disconnected.");
    }
}
