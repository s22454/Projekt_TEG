using UnityEngine;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class PipeSystem : MonoBehaviour
{
    public static PipeSystem Instance;
    private static readonly string _className = "PIPE SYSTEM";

    // Class variables
    private Thread _pipeThreadRead;
    private Thread _pipeThreadWrite;
    private NamedPipeClientStream _pipeClientRead;
    private NamedPipeClientStream _pipeClientWrite;
    private volatile bool _active;
    private bool _readerActive;
    private bool _writerActive;
    private string _messageToSend;
    private string _messageRecived;
    private DialogueUIController dialogueUI;

    // Config variables
    public static string pipeNameRead = "read";
    public static string pipeNameWrite = "write";
    private static readonly int frameRate = 5;
    private static int sleepTime;
    private static Dictionary<EnumType, Dictionary<Enum, string>> MessageDataTranslations;

    // Events

    // Message struct
    public struct MessageStruct
    {
        public bool Ready { get; set; }
        public ActionCode ActionCode { get; set; }
        public Sender Sender { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string Message { get; set; }
    }

    void Start()
    {
        if (Instance == null) Instance = this;

        // Initialize variables
        _pipeThreadRead = new Thread(PipeReader);
        _pipeThreadWrite = new Thread(PipeWriter);
        _active = true;
        _messageToSend = "";
        _messageRecived = "";
        sleepTime = 1000 / frameRate;

        // Get configuration
        InitConfig();

        // Connect to pipe server
        _pipeClientRead = new NamedPipeClientStream(".", pipeNameRead, PipeDirection.In);
        _pipeClientRead.Connect(5000);
        _pipeClientWrite = new NamedPipeClientStream(".", pipeNameWrite, PipeDirection.Out);
        _pipeClientWrite.Connect(5001);
        LogManager.Log(_className, LogType.LOG, "Connected to python pipe");

        // Create and start threads
        _pipeThreadRead.Start();
        _pipeThreadWrite.Start();

        DialogueUIController.InitializePipeSystem(Instance);
    }

    void Awake()
    {
        DialogueUIController.InitializePipeSystem(Instance);
    }

    // Read json config file and init dictionary
    private void InitConfig()
    {
        // get data from file
        string json = File.ReadAllText("./Assets/Scripts/Constants/PipeMessageDataTranslations.json");

        // deserialize raw json
        var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

        // final result
        MessageDataTranslations = new Dictionary<EnumType, Dictionary<Enum, string>>();

        foreach (var (key, innerDict) in raw)
        {
            // Parse enum type
            if (!Enum.TryParse<EnumType>(key, true, out var enumTypeKey))
            {
                LogManager.Log(_className, LogType.WARNING, "Error while reading PipeMessageDataTranslations");
                continue;
            }

            // Enum values
            var mappedDict = new Dictionary<Enum, string>();

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
                    LogManager.Log(_className, LogType.WARNING, "Error while translating enum value");
                    continue;
                }

                // add enum code translations to dictionary
                mappedDict.Add(parsedEnum, code);
            }

            // add enum values to dictionary
            MessageDataTranslations.Add(enumTypeKey, mappedDict);
        }
    }


    void PipeReader()
    {
        _readerActive = true;
        string methodName = _className + " READ";

        try
        {
            using (var sr = new StreamReader(_pipeClientRead))
            {
                var readAsync = sr.ReadLineAsync();

                do
                {
                    if (readAsync.IsCompleted)
                    {
                        // decode message
                        _messageRecived = readAsync.Result;
                        LogManager.Log(methodName, LogType.LOG, $"Got message encoded: {_messageRecived}");
                        MessageStruct messageDecoded = DecodeMessage(_messageRecived);
                        string messageDecodedLog = "Got message decoded: " +
                            messageDecoded.ActionCode + "|" +
                            messageDecoded.Sender + "|" +
                            messageDecoded.Item + "|" +
                            messageDecoded.Quantity + "|" +
                            messageDecoded.Price + "|" +
                            messageDecoded.Message;

                        LogManager.Log(methodName, LogType.LOG, messageDecodedLog);

                        // call event
                        LogManager.Log(methodName, LogType.LOG, "Invoking OnMessageRecived");
                        messageDecoded.Ready = true;
                        DialogueUIController._messageRecived = messageDecoded;

                        // create new read task
                        readAsync.Dispose();
                        readAsync = sr.ReadLineAsync();
                    }

                    // frame rate limiter
                    Thread.Sleep(sleepTime);

                } while (_active);
            }
        }
        catch (Exception e)
        {
            LogManager.Log(methodName, LogType.ERROR, e.Message);
        }

        _readerActive = false;
    }

    void PipeWriter()
    {
        _writerActive = true;
        string methodName = _className + " WRITE";

        try
        {
            using (var sw = new StreamWriter(_pipeClientWrite) { AutoFlush = true })
            {
                do
                {
                    if (_messageToSend.Length > 0)
                    {
                        LogManager.Log(methodName, LogType.LOG, $"Sending message: {_messageToSend}");
                        sw.WriteLine(_messageToSend);
                        LogManager.Log(methodName, LogType.LOG, $"{_messageToSend} sent");
                        _messageToSend = "";
                    }

                    // frame rate limiter
                    Thread.Sleep(sleepTime);

                } while (_active && !_messageRecived.Equals("exit"));
            }
        }
        catch (Exception e)
        {
            LogManager.Log(methodName, LogType.ERROR, e.Message);
        }

        _writerActive = false;
    }

    private bool SendMessageToServer(string message)
    {
        if (_pipeClientWrite is not null && _pipeClientWrite.IsConnected && _active && _writerActive)
        {
            _messageToSend = message;
            return true;
        }

        return false;
    }

    public bool EncodeAndSendMessageToServer(ActionCode code, Sender sender, Item item, int quantity = 0, int price = 0, string message = "")
    {
        string encodedMessage =
            MessageDataTranslations[EnumType.ActionCode][code] + "|" +
            MessageDataTranslations[EnumType.Sender][sender] + "|" +
            MessageDataTranslations[EnumType.Item][item] + "|" +
            quantity.ToString() + "|" +
            price.ToString() + "|" +
            message + "\n"
        ;

        return SendMessageToServer(encodedMessage);
    }

    public bool EncodeAndSendMessageToServer(MessageStruct messageStruct)
    {
        return EncodeAndSendMessageToServer(
            code: messageStruct.ActionCode,
            sender: messageStruct.Sender,
            item: messageStruct.Item,
            quantity: messageStruct.Quantity,
            price: messageStruct.Price,
            message: messageStruct.Message
        );
    }

    private MessageStruct DecodeMessage(string message)
    {
        // Message format
        // [action code] | [sender] | [item] | [quantity] | [price] | [message]
        //       0             1        2          3           4          5

        // Split message into managable pieces
        string[] messageParts = message.Split("|");

        // Get ActionCode
        var (actionKey, actionCode) = MessageDataTranslations[EnumType.ActionCode]
            .SingleOrDefault(a => a.Value.Equals(messageParts[0]));

        // Get Sender
        var (senderKey, senderCode) = MessageDataTranslations[EnumType.Sender]
            .SingleOrDefault(s => s.Value.Equals(messageParts[1]));

        // Get Item
        var (itemKey, itemCode) = MessageDataTranslations[EnumType.Item]
            .SingleOrDefault(i => i.Value.Equals(messageParts[2]));

        MessageStruct msgStruct = new MessageStruct
        {
            ActionCode = (ActionCode)actionKey,
            Sender = (Sender)senderKey,
            Item = (Item)itemKey,
            Quantity = int.Parse(messageParts[3]),
            Price = int.Parse(messageParts[4]),
            Message = messageParts[5]
        };

        LogManager.Log(_className, LogType.LOG, "Decoding message");

        if (msgStruct.Item != Item.NULL && msgStruct.Item != Item.TEST && msgStruct.Item != Item.GOLD)
        {
            InventoryManager.AddItem(msgStruct.Item);
            InventoryManager.RemoveItem(Item.GOLD, DialogueUIController._itemCosts[msgStruct.Item]);
        }

        return msgStruct;
    }

    void OnApplicationQuit()
    {
        try
        {
            // send closing message
            _messageToSend = "exit";
            _messageRecived = "exit";
            _active = false;

            // wait for message to be sent
            while (_writerActive && _readerActive)
                Thread.Sleep(100);

            // close thread if it still exisits
            _pipeThreadRead?.Join();
            _pipeThreadWrite?.Join();

            // close clinet if it still exisits
            _pipeClientRead?.Close();
            _pipeClientWrite?.Close();
        }
        catch (Exception e)
        {
            LogManager.Log(_className, LogType.ERROR, $"Error while closing: {e.Message}");
        }
    }

    // Test method
    [ContextMenu("Send test message")]
    private void SendTestMessage()
    {
        LogManager.Log(_className, LogType.LOG, "Sending test message");

        if (EncodeAndSendMessageToServer(ActionCode.TESTMESSAGE, Sender.TEST, Item.TEST))
            LogManager.Log(_className, LogType.LOG, "Sending test message successful");
        else
            LogManager.Log(_className, LogType.LOG, "Sending test message failed");
    }
}
