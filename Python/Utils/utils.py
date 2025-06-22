from .message_enums import MessageType as mt
from rich import print
from datetime import datetime
import config

log_path = config.LOG_PATH

def Log(sender:str, type:mt, msg:str):
    message_color_start = ""
    message_color_end = ""

    # construct message
    now = datetime.now().strftime("%H:%M:%S")
    message = f"[{now}] [{sender}] | {msg}"

    # print message to console
    match type:
        case mt.LOG:
            message_color_start = "[white]"
            message_color_end = "[/white]"

        case mt.WARNING:
            message_color_start = "[yellow]"
            message_color_end = "[/yellow]"

        case mt.ERROR:
            message_color_start = "[red]"
            message_color_end = "[/red]"

    print(f"{message_color_start}{message}{message_color_end}")

    # save message to log
    with open(log_path, "a") as file:
        file.write(f"{message}\n")

def InitLog():
    # get current date time
    now = datetime.now().strftime("%d/%m/%Y %H:%M:%S")

    # construct message
    message = "---------------------------------------"
    message += f"\nSTARTING PROJECT - {now}"
    message += "\n---------------------------------------"

    # print start to console
    print("[green]" + message + "[/green]")

    # save start to log file
    with open(log_path, "a") as file:
        file.write(f"\n{message}\n")
