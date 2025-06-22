from message_enums import MessageType as mt
from rich import print
from datetime import datetime

def Log(sender:str, type:mt, msg:str):
    message_colour_start = ""
    message_colour_end = ""

    # construct message
    now = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
    message = f"[{now}] [{sender}] | {msg}"

    # print message to console
    match type:
        case mt.LOG:
            message_colour_start = "[white]"
            message_colour_end = "[/white]"

        case mt.WARNING:
            message_colour_start = "[yellow]"
            message_colour_end = "[/yellow]"

        case mt.ERROR:
            message_colour_start = "[red]"
            message_colour_end = "[/red]"

    print(f"{message_colour_start}{message}{message_colour_end}")

    # save message to log
    with open("./../log_py.txt", "a") as file:
        file.write(f"{message}\n")
