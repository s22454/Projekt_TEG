using UnityEngine;
using TMPro;
using System.Text;

public class InventoryUIController : UIController
{
    public TMP_Text inventoryText;

    public new void OpenDialogue()
    {
        Debug.Log("InventoryUIController Open called");
        if (Input.GetKeyDown(KeyCode.I))
        {
            gameObject.SetActive(!gameObject.activeSelf);
            if (gameObject.activeSelf)
            {
                infoPanel.SetActive(false);
                RefreshUI();
            }
        }
    }

    void RefreshUI()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in InventoryManager.Instance.items)
        {
            sb.AppendLine($"{item.quantity}x {item.itemName}");
        }

        inventoryText.text = sb.Length > 0 ? sb.ToString() : "Inventory is empty.";
    }
}