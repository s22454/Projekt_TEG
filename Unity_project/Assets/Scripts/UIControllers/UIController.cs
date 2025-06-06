using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public abstract class UIController : MonoBehaviour
{
    public GameObject infoPanel;

    void Start()
    {
        StartCoroutine(HideAfterOneFrame());
    }

    System.Collections.IEnumerator HideAfterOneFrame()
    {
        yield return null;
        this.CloseDialogue();
    }

    public void OpenDialogue()
    {
        gameObject.SetActive(true);
        infoPanel.SetActive(false);
    }

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
        infoPanel.SetActive(true);
    }
}