using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public Item itemType;
    public int quantity;

    public InventoryItem(Item type, int qty = 1)
    {
        itemType = type;
        quantity = qty;
    }
}