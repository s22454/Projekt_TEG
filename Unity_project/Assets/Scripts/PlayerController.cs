using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;
    public TMP_InputField dialogueInputField;


    private Vector2 movement;
    private InventoryUIController inventoryUI;

    void Start()
    {
        inventoryUI = FindObjectOfType<InventoryUIController>();
    }

    void Update()
    {
        if (dialogueInputField != null && dialogueInputField.isFocused)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.EndDay();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.OpenDialogue();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}