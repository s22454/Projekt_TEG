using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class DialogueUIController : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;
    private GameObject currentNPC;

    void Start()
    {
        StartCoroutine(HideAfterOneFrame());
    }

    IEnumerator HideAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void OpenDialogue(GameObject npc)
    {
        currentNPC = npc;
        gameObject.SetActive(true);
        StartCoroutine(GetInitialMessage());
    }

    IEnumerator GetInitialMessage()
    {
        string url = $"http://localhost:5000/message?npc={UnityWebRequest.EscapeURL(currentNPC.name)}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (!www.result.Equals(UnityWebRequest.Result.Success))
        {
            dialogueText.text = "Error getting initial message: " + www.error;
        }
        else
        {
            dialogueText.text = www.downloadHandler.text;
        }
    }

    public void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            StartCoroutine(SendMessageToAPI(message));
            inputField.text = "";
        }
    }

    IEnumerator SendMessageToAPI(string message)
    {
        string npcName = currentNPC != null ? currentNPC.name : "unknown";

        string jsonBody = JsonUtility.ToJson(new MessagePayload
        {
            npc = npcName,
            userMessage = message
        });

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest www = new UnityWebRequest("http://localhost:5000/message", "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (!www.result.Equals(UnityWebRequest.Result.Success))
        {
            dialogueText.text = "Error: " + www.error;
        }
        else
        {
            dialogueText.text = www.downloadHandler.text;
        }
    }

    [System.Serializable]
    public class MessagePayload
    {
        public string npc;
        public string userMessage;
    }

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
    }
}