using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEngine;

public class PipeSystem : MonoBehaviour
{
    private Thread _pipeThread;
    private NamedPipeClientStream _pipeClient;

    void Start()
    {
        _pipeThread = new Thread(PipeWorker);
        _pipeThread.Start();
    }

    void PipeWorker()
    {
        try
        {
            // connect to pipe server
            _pipeClient = new NamedPipeClientStream(".", "UnityPipe", PipeDirection.InOut);
            _pipeClient.Connect(5000);
            Debug.log("PIPESYSTEM | Connected to python pipe");

            // sending message
            string message = "test1"; //TODO implement custom message sending
            SendMessageToServer(message);

            // reading response
            List<byte> buffer = new List<byte>();
            int bytesRead = _pipeClient.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.log("PIPESYSTEM | incoming message: " + response);
        }
        catch (System.Exception)
        {
            Debug.LogError("PIPESYSTEM | " + e.message);
        }
    }

    private void SendMessageToServer(string message)
    {
        if (_pipeClient is not null && _pipeClient.IsConnected)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _pipeClient.Write(data, 0, data.Length);
            _pipeClient.Flush();
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            // send closing message
            SendMessageToServer("exit");

            // close clinet if it still exisits
            _pipeClient?.Close();
        }
        catch (System.Exception)
        {
            Debug.LogError("PIPESYSTEM | Error while closing: " + e.message);
        }

        // close thread if it still exisits
        _pipeThread?.Join();
    }
}
