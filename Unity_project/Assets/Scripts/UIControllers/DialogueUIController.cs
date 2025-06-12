using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Diagnostics;
using static PipeSystem;

public class DialogueUIController : UIController
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private string npcId;
    public UnityEngine.UI.Image portraitImage;
    public Sprite smithPortrait, merchantPortrait, herbalistPortrait;
    private PipeSystem pipeSystem;

    void Start()
    {
        StartCoroutine(base.HideAfterOneFrame());
        UnityEngine.Debug.Log("Start DialogueUIController");
        PipeSystem.OnPipeInitialize += InitializePipeSystem;
    }

    public void InitializePipeSystem(PipeSystem pipeSystem)
    {
        this.pipeSystem = pipeSystem;
        pipeSystem.OnMessageRecived += UpdateDialogueText;
    }

    public void OpenDialogue(string npc)
    {
        base.OpenDialogue();
        npcId = npc;

        switch (npcId)
        {
            case "Smith": portraitImage.sprite = smithPortrait; break;
            case "Merchant": portraitImage.sprite = merchantPortrait; break;
            case "Herbalist": portraitImage.sprite = herbalistPortrait; break;
            default: portraitImage.sprite = null; break;
        }

        // Send initial greeting through pipe
        pipeSystem.EncodeAndSendMessageToServer(ActionCode.TXTMESSAGE, Sender.PLAYER, Item.NULL, 0, 0, "Witaj!");
    }

    public void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            pipeSystem.EncodeAndSendMessageToServer(ActionCode.TXTMESSAGE, Sender.PLAYER, Item.NULL, 0, 0, message);
            inputField.text = "";
            dialogueText.text = "...";
        }
    }

    void OnDestroy()
    {
        if (pipeSystem != null)
            pipeSystem.OnMessageRecived -= UpdateDialogueText;
    }

    void UpdateDialogueText(MessageStruct msg)
    {
        UnityEngine.Debug.Log("UpdateDialogueText");
        if (msg.ActionCode == ActionCode.TXTMESSAGE)
            dialogueText.text = msg.Message;
    }
}