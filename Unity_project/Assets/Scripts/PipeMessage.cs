using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

public class PipeMessage
{
    public Action action;
    public string transactionItem;
    public int itemQuantity;
    public int itemPrice;
    public string dialogueLine;

    public PipeMessage(Action act, string item, int quantity, int price, string msg)
    {
        action = act;
        transactionItem = item;
        itemQuantity = quantity;
        itemPrice = price; 
        dialogueLine = msg;
    }
}

public enum Action : int
{
    Message,
    Buy
}

