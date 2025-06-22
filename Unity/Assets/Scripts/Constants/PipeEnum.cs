using UnityEngine;

// [action code] | [sender] | [reciver] | [item] | [quantity] | [price] | [message]
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
    ENDCONVARSATION,
    ENDDAY
}

public enum Sender
{
    TEST,
    SMITH,
    BAKER,
    HERBALIST,
    PLAYER,
    SYSTEM
}
public enum Item
{
    TEST,
    SWORD,
    BREAD,
    WEED,
    GOLD,
    NULL
}
