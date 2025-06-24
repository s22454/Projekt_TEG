using UnityEngine;
using System.Collections;

public abstract class UIController : MonoBehaviour
{
    public GameObject infoPanel;
    public bool _dialogOpened;

    void Start()
    {
        StartCoroutine(HideAfterOneFrame());
    }

    public IEnumerator HideAfterOneFrame()
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
