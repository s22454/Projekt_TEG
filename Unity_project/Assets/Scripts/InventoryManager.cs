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

        items.Add(new InventoryItem("Gold", 100));
        items.Add(new InventoryItem("Silver", 1000));
        items.Add(new InventoryItem("Copper", 987654321));
    }

    public void AddItem(Item item, int amount = 1)
    {
        InventoryItem existing = items.Find(i => i.itemName == "itemName");
        if (existing != null)
        {
            existing.quantity += amount;
        }
        else
        {
            items.Add(new InventoryItem("itemName", amount));
        }

        Debug.Log($"Added {amount}x {"itemName"}");
    }

    public void RemoveItem(string item, int amount = 1)
    {
        InventoryItem existing = items.Find(i => i.itemName == "itemName");
        if (existing != null)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0)
                items.Remove(existing);

            Debug.Log($"Removed {amount}x {"itemName"}");
        }
    }
}