using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class DialogueUIController : UIController
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private string npcId;
    public UnityEngine.UI.Image portraitImage;
    public Sprite smithPortrait, merchantPortrait, herbalistPortrait;

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
        PipeSystem.Instance.SendMessageToServer($"{ActionCode.TXTMESSAGE}|{Sender.PLAYER}|null|0|0|Witaj!");
    }

    public void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            PipeSystem.Instance.SendMessageToServer($"{ActionCode.TXTMESSAGE}|{Sender.PLAYER}|null|0|0|{message}"); //co z odbiorc¹?
            inputField.text = "";
            dialogueText.text = "...";
        }
    }

    public void UpdateDialogueText(string text)
    {
        dialogueText.text = text ?? "No response";
    }
}