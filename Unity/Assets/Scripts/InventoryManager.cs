using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static readonly string _className = "INVENTORY MANAGER";
    public static InventoryManager Instance;

    private static List<InventoryItem> _items = new List<InventoryItem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _items.Add(new InventoryItem(Item.GOLD, 100));
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public static void AddItem(Item item, int amount = 1)
    {
        if (amount <= 0) return;

        InventoryItem existing = _items.Find(i => i.itemType == item);
        if (existing != null)
        {
            existing.quantity += amount;
        }
        else
        {
            _items.Add(new InventoryItem(item, amount));
        }

        LogManager.Log(_className, LogType.LOG, $"Added {amount}x {item}");
    }

    public static void RemoveItem(Item item, int amount = 1)
    {
        if (amount <= 0) return;

        InventoryItem existing = _items.Find(i => i.itemType == item);
        if (existing != null)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0 && existing.itemType != Item.GOLD)
                _items.Remove(existing);

            LogManager.Log(_className, LogType.LOG, $"Removed {amount}x {item}");
        }
        else
        {
            LogManager.Log(_className, LogType.WARNING, $"Tried to remove {item}, but not found in inventory.");
            return;
        }
    }

    public static List<InventoryItem> GetItems()
    {
        return _items;
    }
}
