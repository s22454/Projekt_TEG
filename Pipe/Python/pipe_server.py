import win32pipe, win32file, pywintypes
import threading
import time


def test_message_print(message):
    print(f"[TEST MESSAGE] Message: {message}")



class PipeServer:

    # Singleton instance
    _instance = None

    # Pipe variables
    pipe_name = r'\\.\pipe\test_pipe'
    pipe = None
    pipe_thread = None
    stop_event = None
    message = ""

    # Singleton constructor
    def __new__(cls):
        if not cls._instance:
            cls._instance = super().__new__(cls)
            cls._instance.stop_event = threading.Event()
        return cls._instance

    # Start pipe server on new thread
    def start(self):
        print(f"[PIPE SERVER] Starting pipe server on new thread")
        self.pipe_thread = threading.Thread(target=self.__run, daemon=True)
        self.pipe_thread.start()

    # Run pipe server logic
    def __run(self):
        print(f"[PIPE SERVER] Server is running")

        while not self.stop_event.is_set() and self.message != "exit":
            # create pipe
            self.pipe = win32pipe.CreateNamedPipe(
                self.pipe_name,
                win32pipe.PIPE_ACCESS_DUPLEX,
                win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
                1, 65536, 65536,
                0,
                None
            )

            # connect to client
            print("[PIPE SERVER] Waiting for client...")
            try:
                win32pipe.ConnectNamedPipe(self.pipe, None)
                print("[PIPE SERVER] Client connected")

                # read messages
                _, data = win32file.ReadFile(self.pipe, 1024)
                self.message = data.decode()
                print(f"[PIPE SERVER] Got message: {self.message}")
                test_message_print(self.message)

                # process message
                print(self.message)
                if '|' in self.message:
                    npc, user_msg = self.message.split('|', 1)
                    response = f"{npc} got your message: {user_msg}"
                elif "EndDay" in self.message:
                    print("[PIPE SERVER] EndDay, Agents exchange info")
                    response = "Agents exchange info"
                else:
                    response = f"Invalid message format: {self.message}"
                win32file.WriteFile(self.pipe, response.encode('utf-8'))

            except pywintypes.error as e:
                if e.winerror in [109, 233]: # ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED
                    print("[PIPE SERVER] Client has disconnected")
                    self.message = "exit"
                else:
                    print(f"[PIPE SERVER] Reading error {e}")
                break
            # close pipe
            finally:
                win32file.CloseHandle(self.pipe)
                self.pipe = None

    # Stop pipe server
    def stop(self):
        print("[PIPE SERVER] Stopping pipe server...")
        time.sleep(1) #TODO Wait for c# to close connection
        self.stop_event.set()
        self.pipe_thread.join()

# test
pipe_server = PipeServer()
pipe_server.start()

while pipe_server.message != "exit":
    time.sleep(0.5)

pipe_server.stop()
