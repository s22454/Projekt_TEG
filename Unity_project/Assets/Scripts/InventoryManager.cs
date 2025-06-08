using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        items.Add(new InventoryItem(Item.GOLD, 100));
    }

    public void AddItem(Item item, int amount = 1)
    {
        InventoryItem existing = items.Find(i => i.itemType == item);
        if (existing != null)
        {
            existing.quantity += amount;
        }
        else
        {
            items.Add(new InventoryItem(item, amount));
        }

        Debug.Log($"Added {amount}x {"itemName"}");
    }

    public void RemoveItem(Item item, int amount = 1)
    {
        InventoryItem existing = items.Find(i => i.itemType == item);
        if (existing != null)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0 && existing.itemType != Item.GOLD)
                items.Remove(existing);

            Debug.Log($"Removed {amount}x {"itemName"}");
        }
    }
}