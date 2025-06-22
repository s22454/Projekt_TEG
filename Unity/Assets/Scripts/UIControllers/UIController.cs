using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public abstract class UIController : MonoBehaviour
{
    public GameObject infoPanel;
    public bool _dialogOpened;

    void Start()
    {
        StartCoroutine(HideAfterOneFrame());
    }

    public System.Collections.IEnumerator HideAfterOneFrame()
    {
        yield return null;
        this.CloseDialogue();
    }

    public void OpenDialogue()
    {
        gameObject.SetActive(true);
        infoPanel.SetActive(false);
        _dialogOpened = true;
    }

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
        infoPanel.SetActive(true);
        _dialogOpened = false;
    }
}
