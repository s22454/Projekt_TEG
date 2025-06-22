from NPC_Manager import npc_manager
import threading
from Utils import InitLog

npc_data_folder = "./NPC_Manager/Data"

if __name__ == "__main__":
    InitLog()
    npc_manager = npc_manager.NPCManager(data_folder = npc_data_folder)
    try:
        while not npc_manager.pipe_server.stop_event_write.is_set():
            threading.Event().wait(0.5)
    except KeyboardInterrupt:
        npc_manager.pipe_server.Stop()
