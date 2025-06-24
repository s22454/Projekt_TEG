using UnityEngine;
using TMPro;
using static PipeSystem;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DialogueUIController : UIController
{
    private static readonly string _className = "DIALOGUE UI CONTROLLER";
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private string npcId;
    public UnityEngine.UI.Image portraitImage;
    public Sprite smithPortrait, merchantPortrait, herbalistPortrait;
    private static PipeSystem _pipeSystem;
    public static bool _isReady;
    public static MessageStruct _messageRecived;
    private Sender _currentNPC;
    public static Dictionary<Item, int> _itemCosts;

    void Start()
    {
        StartCoroutine(base.HideAfterOneFrame());
        LogManager.Log(_className, LogType.LOG, "Started");

        _itemCosts = new()
        {
            {Item.TEST, 0},
            {Item.SWORD, 20},
            {Item.BREAD, 40},
            {Item.WEED, 60},
            {Item.GOLD, 0},
            {Item.NULL, 0},
        };
    }

    public static void InitializePipeSystem(PipeSystem pipeSystem)
    {
        _pipeSystem = pipeSystem;
        _isReady = true;
    }

    public void OpenDialogue(string npc)
    {
        base.OpenDialogue();
        npcId = npc;


        switch (npcId)
        {
            case "Smith": portraitImage.sprite = smithPortrait; _currentNPC = Sender.SMITH; break;
            case "Merchant": portraitImage.sprite = merchantPortrait; _currentNPC = Sender.BAKER; break;
            case "Herbalist": portraitImage.sprite = herbalistPortrait; _currentNPC = Sender.HERBALIST; break;
            default: portraitImage.sprite = null; break;
        }

        // Send initial greeting through pipe
        if (_pipeSystem != null)
            _pipeSystem.EncodeAndSendMessageToServer(ActionCode.TXTMESSAGE, _currentNPC, Item.NULL, 0, 0, "Witaj!");
        else
            LogManager.Log(_className, LogType.ERROR, "_pipeSystem is null in OpenDialogue");
    }

    public void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            _pipeSystem.EncodeAndSendMessageToServer(ActionCode.TXTMESSAGE, _currentNPC, Item.NULL, 0, 0, message);
            inputField.text = "";
            dialogueText.text = "...";
        }
    }

    public void BuyItem()
    {
        Item item = _currentNPC switch
        {
            Sender.TEST => Item.TEST,
            Sender.BAKER => Item.BREAD,
            Sender.HERBALIST => Item.WEED,
            Sender.SMITH => Item.SWORD,
            Sender.PLAYER => Item.GOLD,
            _ => Item.NULL
        };

        MessageStruct message = new()
        {
            ActionCode = ActionCode.SELL,
            Sender = _currentNPC,
            Item = item,
            Quantity = 1,
            Price = _itemCosts[item],
            Message = $"Chciałbym kupić {item.HumanName()} za {_itemCosts[item]}"
        };

        _pipeSystem.EncodeAndSendMessageToServer(message);
        inputField.text = "";
        dialogueText.text = "...";
    }

    void Update()
    {
        if (_messageRecived.Ready)
        {
            UpdateDialogueText(_messageRecived);
            _messageRecived.Ready = false;
        }
    }

    void UpdateDialogueText(MessageStruct msg)
    {
        if (msg.ActionCode == ActionCode.TXTMESSAGE)
            dialogueText.text = msg.Message;
    }

    public new void CloseDialogue()
    {
        base.CloseDialogue();
        inputField.text = "";
        dialogueText.text = "...";
    }
}
