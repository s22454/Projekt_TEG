using UnityEngine;

// [action code] | [sender] | [item] | [quantity] | [price] | [message]
public enum EnumType
{
    ActionCode,
    Sender,
    Item
}

public enum ActionCode
{
    TESTMESSAGE,
    TXTMESSAGE,
    SELL,
    CONFIRMSELL,
    ENDCONVARSATION
}

public enum Sender
{
    TEST,
    SMITH,
    BAKER,
    HERBALIST,
    PLAYER,
}
public enum Item
{
    TEST,
    SWORD,
    BREAD,
    WEED
}
