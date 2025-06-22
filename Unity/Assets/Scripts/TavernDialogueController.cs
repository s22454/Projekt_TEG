using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;


public class TavernDialogueController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitAndStartNewDay());
    }

    IEnumerator WaitAndStartNewDay()
    {
        //PipeMessenger.SendMessage("EndDay");
        yield return new WaitForSeconds(10);
        GameManager.Instance.StartNewDay();
    }
}