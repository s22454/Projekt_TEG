using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int quantity;

    public InventoryItem(string name, int qty = 1)
    {
        itemName = name;
        quantity = qty;
    }
}