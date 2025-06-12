using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Diagnostics;
using static PipeSystem;
using System;

public class DialogueUIController : UIController
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private string npcId;
    public UnityEngine.UI.Image portraitImage;
    public Sprite smithPortrait, merchantPortrait, herbalistPortrait;
    private static PipeSystem _pipeSystem;
    public static bool _isReady;
    public static MessageStruct _messageRecived;
    private Sender _currentNPC;

    void Start()
    {
        StartCoroutine(base.HideAfterOneFrame());
        UnityEngine.Debug.Log("Start DialogueUIController");
    }

    public static void InitializePipeSystem(PipeSystem pipeSystem)
    {
        UnityEngine.Debug.Log("xdddddddddd");
        _pipeSystem = pipeSystem;
        _isReady = true;
    }

    public void OpenDialogue(string npc)
    {
        base.OpenDialogue();
        npcId = npc;

        UnityEngine.Debug.Log("WTFFFFFFFFFFFFFFFFFFFF");

        switch (npcId)
        {
            case "Smith": portraitImage.sprite = smithPortrait; _currentNPC = Sender.SMITH; break;
            case "Merchant": portraitImage.sprite = merchantPortrait; _currentNPC = Sender.BAKER; break;
            case "Herbalist": portraitImage.sprite = herbalistPortrait; _currentNPC = Sender.HERBALIST; break;
            default: portraitImage.sprite = null; break;
        }

        // Send initial greeting through pipe
        _pipeSystem.EncodeAndSendMessageToServer(ActionCode.TXTMESSAGE, _currentNPC, Item.NULL, 0, 0, "Witaj!");
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
        UnityEngine.Debug.Log("UpdateDialogueText");
        if (msg.ActionCode == ActionCode.TXTMESSAGE)
            dialogueText.text = msg.Message;
    }
}
