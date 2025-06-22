from enum import Enum
from .pipe_enums import ActionCode, Sender, Item
from dataclasses import dataclass

@dataclass
class Message:
    action_code: ActionCode
    sender: Sender
    item: Item
    quantity: int = 0
    price: int = 0
    message: str = ""
