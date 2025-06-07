using UnityEngine;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class PipeSystem : MonoBehaviour
{
    // Class variables
    private Thread _pipeThread;
    private NamedPipeClientStream _pipeClient;
    private bool _active;
    private string _message;
    private string _response;
    private PipeState _pipeState;

    // Config variables
    public static string pipeName = "test_pipe";
    private static readonly int frameRate = 5;
    private static int sleepTime;
    private static Dictionary<EnumType, Dictionary<string, Enum>> MessageDataTranslations;

    // State enum
    private enum PipeState
    {
        WRITE,
        READ
    }

    // Message struct
    public struct MessageStruct
    {
        public ActionCode ActionCode { get; set; }
        public Sender Sender { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string Message { get; set; }
    }

    void Start()
    {
        _pipeThread = new Thread(PipeWorker);
        _active = true;
        _message = "";
        _response = "";
        sleepTime = 1000 / frameRate;
        InitConfig();
        _pipeThread.Start();
    }

    // Read json config file and init dictionary
    private void InitConfig()
    {
        // get data from file
        string json = File.ReadAllText("./Assets/Scripts/Constants/PipeMessageDataTranslations.json");

        // deserialize raw json
        var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

        // final result
        MessageDataTranslations = new Dictionary<EnumType, Dictionary<string, Enum>>();

        foreach (var (key, innerDict) in raw)
        {
            // Parse enum type
            if (!Enum.TryParse<EnumType>(key, true, out var enumTypeKey))
            {
                Debug.LogError("PIPESYSTEM | Error while reading PipeMessageDataTranslations");
                continue;
            }

            // Enum values
            var mappedDict = new Dictionary<string, Enum>();

            // Get enum value
            foreach (var (code, valueStr) in innerDict)
            {
                Enum parsedEnum = null;

                switch (enumTypeKey)
                {
                    case EnumType.ActionCode:
                        Enum.TryParse<ActionCode>(valueStr, true, out var parsedAction);
                        parsedEnum = parsedAction;
                        break;

                    case EnumType.Sender:
                        Enum.TryParse<Sender>(valueStr, true, out var parsedSender);
                        parsedEnum = parsedSender;
                        break;

                    case EnumType.Item:
                        Enum.TryParse<Item>(valueStr, true, out var parsedItem);
                        parsedEnum = parsedItem;
                        break;

                    default:
                        break;
                }

                // check enum
                if (parsedEnum is null)
                {
                    Debug.LogError("PIPESYSTEM | Error while translating enum value");
                    continue;
                }

                // add enum code translations to dictionary
                mappedDict.Add(code, parsedEnum);
            }

            // add enum values to dictionary
            MessageDataTranslations.Add(enumTypeKey, mappedDict);
        }
    }


    void PipeWorker()
    {
        try
        {
            // connect to pipe server
            _pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            _pipeClient.Connect(5000);
            Debug.Log("PIPESYSTEM | Connected to python pipe");

            using (var sr = new StreamReader(_pipeClient))
            using (var sw = new StreamWriter(_pipeClient) { AutoFlush = true })
            {
                do
                {
                    switch (_pipeState)
                    {
                        // sending message
                        case PipeState.WRITE:
                            if (_message.Length > 0)
                            {
                                sw.WriteLine(_message);
                                Debug.Log("PIPESYSTEM | Sending message: " + _message);
                                _message = "";
                                _pipeState = PipeState.READ;
                            }
                            break;

                        case PipeState.READ:
                            _response = sr.ReadLine();
                            DecodeMessage(_response);
                            _pipeState = PipeState.WRITE;
                            Debug.Log("PIPESYSTEM | Incoming message: " + _response);
                            break;

                        default:
                            break;
                    }

                    // frame rate limiter
                    Thread.Sleep(sleepTime);

                } while (_active && !_response.Equals("exit"));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("PIPESYSTEM | " + e.Message);
        }
    }

    public bool SendMessageToServer(string message)
    {
        if (_pipeClient is not null && _pipeClient.IsConnected && _active && _pipeState == PipeState.WRITE)
        {
            _message = message;
            return true;
        }

        return false;
    }

    private MessageStruct DecodeMessage(string message)
    {
        // Message format
        // [action code] | [sender] | [item] | [quantity] | [price] | [message]
        //       0             1        2          3           4          5

        // Split message into managable pieces
        string[] messageParts = message.Split("|");

        return new MessageStruct
        {
            ActionCode = (ActionCode)MessageDataTranslations[EnumType.ActionCode][messageParts[0]],
            Sender = (Sender)MessageDataTranslations[EnumType.Sender][messageParts[1]],
            Item = (Item)MessageDataTranslations[EnumType.Item][messageParts[2]],
            Quantity = int.Parse(messageParts[3]),
            Price = int.Parse(messageParts[4]),
            Message = messageParts[5]
        };
    }

    void OnApplicationQuit()
    {
        try
        {
            // send closing message
            _message = "exit";
            _pipeState = PipeState.WRITE;

            // wait for message to be sent
            while (_message.Length > 0)
                Thread.Sleep(100);

            // close clinet if it still exisits
            _pipeClient?.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("PIPESYSTEM | Error while closing: " + e.Message);
        }

        // close thread if it still exisits
        _pipeThread?.Join();
    }

    // Test methods

    [ContextMenu("Send test message")]
    private void SendTestMessage()
    {
        Debug.Log("PIPESYSTEM | Sending test message");
        if (SendMessageToServer("test"))
            Debug.Log("PIPESYSTEM | Sending test message successful");
        else
            Debug.Log("PIPESYSTEM | Sending test message failed");
    }
}
