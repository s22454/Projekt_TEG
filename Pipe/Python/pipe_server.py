import win32pipe, win32file, pywintypes
import threading
import time
from pipe_enums import EnumType, ActionCode, Sender, Item
import json
from message import Message
from pipe_event import Event

class PipeServer:

    # Singleton instance
    _instance = None

    # Pipe variables
    pipe_name_read = r'\\.\pipe\write'
    pipe_name_write = r'\\.\pipe\read'
    pipe_read = None
    pipe_write = None
    pipe_thread_read = None
    pipe_thread_write = None
    stop_event_read = None
    stop_event_write = None
    message = ""
    response:Message = None
    message_data_translations = {}

    # Event
    OnMessageRecived:Event

    # Singleton constructor
    def __new__(cls):
        if not cls._instance:
            cls._instance = super().__new__(cls)
            cls._instance.stop_event_read = threading.Event()
            cls._instance.stop_event_write = threading.Event()
            cls.OnMessageRecived = Event()
        return cls._instance

    # Start pipe server on new thread
    def start(self):
        print(f"[PIPE SERVER] Starting pipe server on new thread")
        self.ImportEnumCodes()
        self.pipe_thread_read = threading.Thread(target=self.Read, daemon=True)
        self.pipe_thread_write = threading.Thread(target=self.Write, daemon=True)
        self.pipe_thread_read.start()
        self.pipe_thread_write.start()

    # Import code translations
    def ImportEnumCodes(self):
        with open("PipeMessageDataTranslations.json") as f:
            js = json.load(f)

            for enumTypeStr, codes in js.items():

                # check if enum type is correct
                if not enumTypeStr in EnumType.__members__:
                    print(f"[PIPE SERVER] Bad enum type {enumTypeStr}")
                    continue

                enumType = EnumType[enumTypeStr]

                # get enum inner translation values
                inner_ditc = {}
                for code, enumValue in codes.items():
                    #print(f"{enumValue} | {code}")
                    if not enumValue in enumType.value.__members__:
                        print(f"[PIPE SERVER] Bad enum value {enumValue}")
                        continue

                    inner_ditc[enumType.value[enumValue]] = code

                self.message_data_translations[enumType] = inner_ditc

    # Run pipe server read logic
    def Read(self):

        print(f"[PIPE SERVER READ] Server read is running")

        # create pipe
        self.pipe_read = win32pipe.CreateNamedPipe(
            self.pipe_name_read,
            win32pipe.PIPE_ACCESS_INBOUND,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None
        )

        # connect to client
        try:
            print("[PIPE SERVER READ] Waiting for client...")
            win32pipe.ConnectNamedPipe(self.pipe_read, None)
            print("[PIPE SERVER READ] Client connected")

            # read messages
            while not self.stop_event_read.is_set() and self.message != "exit":
                try:
                    # get message and decode it
                    _, data = win32file.ReadFile(self.pipe_read, 1024)
                    encoded_response = data.decode().strip()
                    print(f"[PIPE SERVER READ] Got encoded message: {encoded_response}")

                    # decode message
                    self.response = self.DecodeMessage(encoded_response)
                    print(f"[PIPE SERVER READ] Got decoded message: {self.response}")

                    # fire event
                    self.OnMessageRecived.fire(self.response)

                    # check for exit
                    if self.response.action_code == ActionCode.ENDCONVARSATION:
                        self.Stop(self)

                    # response to test message
                    if self.response.action_code == ActionCode.TESTMESSAGE:
                        self.EncodeAndSendToClient(ActionCode.TESTMESSAGE, Sender.TEST, Item.TEST)

                    if self.response.action_code == ActionCode.TXTMESSAGE:
                        self.EncodeAndSendToClient(ActionCode.TXTMESSAGE, Sender.TEST, Item.TEST, 0, 0, "Got message: " + self.response.message)

                    # clear response var
                    self.response = ""

                except pywintypes.error as e:
                    if e.winerror in [109, 233]: # ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED
                        print("[PIPE SERVER READ] Client has disconnected")
                        self.stop_event_read.set()
                    else:
                        print(f"[PIPE SERVER READ] Reading error {e}")
                    break

        # close pipe
        finally:
            win32file.CloseHandle(self.pipe_read)
            self.pipe_read = None

    # Run pipe server write logic
    def Write(self):
        print(f"[PIPE SERVER WRITE] Server is running")

        # create pipe
        self.pipe_write = win32pipe.CreateNamedPipe(
            self.pipe_name_write,
            win32pipe.PIPE_ACCESS_OUTBOUND,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None
        )

        # connect to client
        try:
            print("[PIPE SERVER WRITE] Waiting for client...")
            win32pipe.ConnectNamedPipe(self.pipe_write, None)
            print("[PIPE SERVER WRITE] Client connected")

            # send messages
            while not self.stop_event_write.is_set():
                try:

                    if len(self.message) > 0:
                        win32file.WriteFile(self.pipe_write, (self.message + "\n").encode('utf-8'))
                        print(f"[PIPE SERVER WRITE] Sending message: {self.message}")
                        win32file.FlushFileBuffers(self.pipe_write)
                        print(f"[PIPE SERVER WRITE] Send message: {self.message}")
                        self.message = ''

                except pywintypes.error as e:
                    if e.winerror in [109, 233]: # ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED
                        print("[PIPE SERVER WRITE] Client has disconnected")
                        self.stop_event_read.set()
                    else:
                        print(f"[PIPE SERVER WRITE] Reading error {e}")
                    break

        # close pipe
        finally:
            win32file.CloseHandle(self.pipe_write)
            self.pipe_write = None

    # get value by key from dict
    def GetKeyByValue(self, dict, val):
        for k, v in dict.items():
            if v == val:
                return k
        return None

    # decode message
    def DecodeMessage(self, encoded_message: str):

        # split message
        message_split = encoded_message.split("|")

        # get values
        action_code = self.GetKeyByValue(self.message_data_translations[EnumType.ActionCode], message_split[0])
        sender = self.GetKeyByValue(self.message_data_translations[EnumType.Sender], message_split[1])
        item = self.GetKeyByValue(self.message_data_translations[EnumType.Item], message_split[2])
        quantity = int(message_split[3])
        price = int(message_split[4])
        message = message_split[5]

        return Message(action_code, sender, item, quantity, price, message)

    # encode message
    def EncodeAndSendToClient(self, action_code, sender, item, quantity=0, price=0, message=""):

        # get code values
        action_code_str = self.message_data_translations[EnumType.ActionCode][action_code]
        sender_str = self.message_data_translations[EnumType.Sender][sender]
        item_str = self.message_data_translations[EnumType.Item][item]

        self.message = f"{action_code_str}|{sender_str}|{item_str}|{quantity}|{price}|{message}"

    def EncodeMessageAndSendToClient(self, message:Message):
        self.EncodeAndSendToClient(message.action_code, message.sender, message.item, message.quantity, message.price, message.message)

    # Stop pipe server
    def Stop(self):
        print("[PIPE SERVER] Stopping pipe server...")
        time.sleep(1)
        self.stop_event_read.set()
        self.stop_event_write.set()
        self.pipe_thread_read.join()
        self.pipe_thread_write.join()

# test
pipe_server = PipeServer()
pipe_server.start()

while not pipe_server.stop_event_write.is_set():
    time.sleep(0.5)

pipe_server.Stop()