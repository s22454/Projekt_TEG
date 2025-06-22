using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static readonly string _className = "INVENTORY MANAGER";
    public static InventoryManager Instance;

    public static List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        items.Add(new InventoryItem(Item.GOLD, 100));
    }

    public static void AddItem(Item item, int amount = 1)
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

        LogManager.Log(_className, LogType.LOG, $"Added {amount}x {"itemName"}");
    }

    public static void RemoveItem(Item item, int amount = 1)
    {
        InventoryItem existing = items.Find(i => i.itemType == item);
        if (existing != null)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0 && existing.itemType != Item.GOLD)
                items.Remove(existing);

            LogManager.Log(_className, LogType.LOG, $"Removed {amount}x {"itemName"}");
        }
    }
}
