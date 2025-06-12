from enum import Enum, auto

class ActionCode(Enum):
    TESTMESSAGE = auto()
    TXTMESSAGE = auto()
    SELL = auto()
    CONFIRMSELL = auto()
    ENDCONVARSATION = auto()

class Sender(Enum):
    TEST = auto()
    SMITH = auto()
    BAKER = auto()
    HERBALIST = auto()
    PLAYER = auto()

class Item(Enum):
    TEST = auto()
    SWORD = auto()
    BREAD = auto()
    WEED = auto()

class EnumType(Enum):
    ActionCode = ActionCode
    Sender = Sender
    Item = Item