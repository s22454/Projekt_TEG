using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class DialogueUIController : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private string npcId;

    void Start()
    {
        StartCoroutine(HideAfterOneFrame());
    }

    System.Collections.IEnumerator HideAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void OpenDialogue(string npc)
    {
        npcId = npc;
        gameObject.SetActive(true);

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

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
    }
}