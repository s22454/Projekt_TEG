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

    enum Npc : int
    {
        Merchant = 0,
        Smith = 1,
        Herbalist = 2
    }

    public void OpenDialogue(string npc)
    {
        npcId = npc;
        gameObject.SetActive(true);
        infoPanel.SetActive(false);

        switch (npcId)
        {
            case "Smith": portraitImage.sprite = smithPortrait; break;
            case "Merchant": portraitImage.sprite = merchantPortrait; break;
            case "Herbalist": portraitImage.sprite = herbalistPortrait; break;
            default: portraitImage.sprite = null; break;
        }


        // Send initial greeting through pipe
        string response = PipeMessenger.SendMessage($"{npcId}|Witaj!");
        dialogueText.text = response ?? "No response.";
    }

    public void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            string response = PipeMessenger.SendMessage($"{npcId}|{message}");
            dialogueText.text = response ?? "No response.";
            inputField.text = "";
        }
    }
}