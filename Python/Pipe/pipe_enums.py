from enum import Enum, auto

class ActionCode(Enum):
    TESTMESSAGE = auto()
    TXTMESSAGE = auto()
    SELL = auto()
    CONFIRMSELL = auto()
    ENDCONVARSATION = auto()
    ENDDAY = auto()

class Sender(Enum):
    TEST = auto()
    SMITH = auto()
    BAKER = auto()
    HERBALIST = auto()
    PLAYER = auto()
    SYSTEM = auto()

class Item(Enum):
    TEST = auto()
    SWORD = auto()
    BREAD = auto()
    WEED = auto()
    GOLD = auto()
    NULL = auto()

class EnumType(Enum):
    ActionCode = ActionCode
    Sender = Sender
    Item = Item
