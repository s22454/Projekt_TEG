using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    private bool playerInRange = false;
    private DialogueUIController dialogueUI;

    void Start()
    {
        dialogueUI = FindObjectOfType<DialogueUIController>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            dialogueUI.OpenDialogue(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueUI.CloseDialogue();
        }
    }
}