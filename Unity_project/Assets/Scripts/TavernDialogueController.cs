using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class TavernDialogueController : MonoBehaviour
{
    public TMP_Text dialogueText;

    void Start()
    {
        StartCoroutine(GetTavernDialogue());
    }

    IEnumerator GetTavernDialogue()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/tavern");
        yield return www.SendWebRequest();

        if (!www.result.Equals(UnityWebRequest.Result.Success))
        {
            dialogueText.text = "Error loading tavern conversation.";
        }
        else
        {
            dialogueText.text = www.downloadHandler.text;
        }

        yield return new WaitForSeconds(10);
        GameManager.Instance.StartNewDay();
    }
}