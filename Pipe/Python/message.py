from enum import Enum
from dataclasses import dataclass

from pipe_enums import ActionCode, Sender, Item

# from Pipe.Python.pipe_enums import ActionCode, Sender, Item

@dataclass
class Message:
    action_code: ActionCode
    sender: Sender
    item: Item
    quantity: int = 0
    price: int = 0
    message: str = ""
