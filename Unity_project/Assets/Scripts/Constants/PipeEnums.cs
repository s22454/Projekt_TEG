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
    TXTMESSAGE,
    COMMAND
}

public enum Sender
{
    SMITH,
    BAKER
}
public enum Item
{
    SWORD,
    BREAD
}
