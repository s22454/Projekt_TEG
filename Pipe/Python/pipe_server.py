import win32pipe, win32file, pywintypes
import threading
import time


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
    response = ""

    # Singleton constructor
    def __new__(cls):
        if not cls._instance:
            cls._instance = super().__new__(cls)
            cls._instance.stop_event_read = threading.Event()
            cls._instance.stop_event_write = threading.Event()
        return cls._instance

    # Start pipe server on new thread
    def start(self):
        print(f"[PIPE SERVER] Starting pipe server on new thread")
        self.pipe_thread_read = threading.Thread(target=self.read, daemon=True)
        self.pipe_thread_write = threading.Thread(target=self.write, daemon=True)
        self.pipe_thread_read.start()
        self.pipe_thread_write.start()

    # Run pipe server read logic
    def read(self):

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
                    self.response = data.decode().strip()
                    print(f"[PIPE SERVER READ] Got message: {self.response}")

                    # check for exit
                    if self.response == "exit":
                        self.stop(self)

                    #! tmp added for testing
                    if self.response == "0000|0000|0000|0|0|":
                        self.message = "0000|0000|0000|0|0|\n"


                    #TODO implement real message logic

                except pywintypes.error as e:
                    if e.winerror in [109, 233]: # ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED
                        print("[PIPE SERVER READ] Client has disconnected")
                        self.message = "exit"
                    else:
                        print(f"[PIPE SERVER READ] Reading error {e}")
                    break

        # close pipe
        finally:
            win32file.CloseHandle(self.pipe_read)
            self.pipe_read = None

    # Run pipe server write logic
    def write(self):
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
            while not self.stop_event_write.is_set() and self.response != "exit":
                try:

                    if self.message != "exit" and len(self.message) > 0:
                        win32file.WriteFile(self.pipe_write, (self.message).encode('utf-8'))
                        print(f"[PIPE SERVER WRITE] Sending message: {self.message}")
                        win32file.FlushFileBuffers(self.pipe_write)
                        self.message = ''

                except pywintypes.error as e:
                    if e.winerror in [109, 233]: # ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED
                        print("[PIPE SERVER WRITE] Client has disconnected")
                        self.message = "exit"
                    else:
                        print(f"[PIPE SERVER WRITE] Reading error {e}")
                    break

        # close pipe
        finally:
            win32file.CloseHandle(self.pipe_write)
            self.pipe_write = None

    # Stop pipe server
    def stop(self):
        print("[PIPE SERVER] Stopping pipe server...")
        time.sleep(1)
        self.stop_event_read.set()
        self.stop_event_write.set()
        self.pipe_thread_read.join()
        self.pipe_thread_write.join()

# test
pipe_server = PipeServer()
pipe_server.start()

while pipe_server.message != "exit":
    time.sleep(0.5)

pipe_server.stop()
