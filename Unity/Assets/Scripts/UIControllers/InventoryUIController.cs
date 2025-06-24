using TMPro;
using System.Text;

public class InventoryUIController : UIController
{
    public TMP_Text inventoryText;

    public new void OpenDialogue()
    {
        base.OpenDialogue();
        RefreshUI();
    }

    void RefreshUI()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in InventoryManager.items)
        {
            sb.AppendLine($"{item.quantity}x {item.itemType}");
        }

        inventoryText.text = sb.Length > 0 ? sb.ToString() : "Inventory is empty.";
    }
}
