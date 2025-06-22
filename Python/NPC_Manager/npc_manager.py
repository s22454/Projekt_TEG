import os
import re
import json
import threading
from dotenv import load_dotenv

from Pipe import PipeServer, Message, ActionCode, Sender, Item
from NPC_Rag import RAG


class NPCManager:
    def __init__(self, data_folder):
        self.data_folder = data_folder
        self.npc_data = {}
        self.npc_agents = {}

        load_dotenv()
        self.load_all_npcs()

        self.pipe_server = PipeServer()
        self.pipe_server.OnMessageRecived.subscribe(self.handle_pipe_message)
        self.pipe_server.start()

    def load_all_npcs(self):
        for file in os.listdir(self.data_folder):
            if file.endswith(".json"):
                npc_name = file[:-5]
                path = os.path.join(self.data_folder, file)
                with open(path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                self.npc_data[npc_name] = data
                self.npc_agents[npc_name] = RAG(path)

    def handle_pipe_message(self, message: Message):
        print(f"[NPC MANAGER] Received message from pipe: {message}")
        sender_npc = message.sender.name.lower()
        player_input = message.message.strip()
        quantity = message.quantity
        response_message : Message

        if message.action_code == ActionCode.TXTMESSAGE:
            response = self.talk_to_npc(sender_npc, player_input)

            response_message = Message(
                action_code=ActionCode.TXTMESSAGE,
                sender=Sender.PLAYER,
                item=Item.TEST,
                message=response
            )

        if message.action_code == ActionCode.SELL:
            response_message = self.sell_item(sender_npc, message.item, quantity)

        if message.action_code == ActionCode.ENDDAY:
            # tu niech sie dzieje po stronie pythona co ma sie dziac
            response_message = Message(
                action_code=ActionCode.ENDDAY,
                sender=Sender.PLAYER,
                item=Item.TEST,
                message="EndDay received"
            )

        self.pipe_server.EncodeMessageAndSendToClient(response_message)

    def share_info(self, from_npc, to_npc, message):
        if "rumors" not in self.npc_data[to_npc]:
            self.npc_data[to_npc]["rumors"] = []

        self.npc_data[to_npc]["rumors"].append(f"[Rumor from {from_npc}]: {message}")
        self._save_npc_to_file(to_npc)

    def _save_npc_to_file(self, npc_name):
        path = os.path.join(self.data_folder, f"{npc_name}.json")
        with open(path, "w", encoding="utf-8") as f:
            json.dump(self.npc_data[npc_name], f, ensure_ascii=False, indent=2)

    def _reload_npc_from_file(self, npc_name):
        path = os.path.join(self.data_folder, f"{npc_name}.json")
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        self.npc_data[npc_name] = data
        #self.npc_agents[npc_name].update_npc_data(path)

    def talk_to_npc(self, npc_name, text):
        self._reload_npc_from_file(npc_name)

        agent = self.npc_agents.get(npc_name)
        if not agent:
            return f"NPC '{npc_name}' does not exist or wasn't loaded."

        result, answer = agent.answer(text)

        mentioned_npcs = self._extract_npc_names(text)
        for mentioned in mentioned_npcs:
            if mentioned in self.npc_data:
                sentiment = self._analyze_sentiment(text)
                self._update_attitude_and_share_plotka(npc_name, mentioned, text, sentiment)

        return answer

    def sell_item(self, npc_name, item, quantity=1):
        if npc_name not in self.npc_data:
            return Message(
                action_code=ActionCode.TXTMESSAGE,
                sender=Sender.PLAYER,
                item=Item.TEST,
                message=f"NPC '{npc_name}' does not exist."
            )

        items = self.npc_data[npc_name].get("items", [])
        for it in items:
            if it["name"].lower() == item.name.lower():
                available = int(it.get("quantity", 0))
                if available < quantity:
                    prompt = f"Player tried to buy {quantity}x {it['name']}, but there wasn't enough."
                    _, response = self.npc_agents[npc_name].answer(prompt)
                    return Message(
                        action_code=ActionCode.TXTMESSAGE,
                        sender=Sender.PLAYER,
                        item=Item.TEST,
                        message=response
                    )

                it["quantity"] = available - quantity

                price = self._extract_price_value(it["price"])
                total_price = price * quantity

                self._save_npc_to_file(npc_name)
                self._reload_npc_from_file(npc_name)

                prompt = (
                    f"The player has bought {quantity}x {it['name']} for {total_price} "
                    f"{self.npc_data[npc_name].get('currency', 'coins')}. How should the NPC respond?"
                )
                _, response = self.npc_agents[npc_name].answer(prompt)

                return Message(
                    action_code=ActionCode.TXTMESSAGE,
                    sender=Sender.PLAYER,
                    item=item,
                    message=response
                )

        prompt = f"Player tried to buy item '{item.name.lower()}', but the NPC does not have it."
        _, response = self.npc_agents[npc_name].answer(prompt)

        return Message(
            action_code=ActionCode.TXTMESSAGE,
            sender=Sender.PLAYER,
            item=Item.TEST,
            message=response
        )

    def _extract_price_value(self, price_str):
        match = re.search(r"\d+", price_str)
        return int(match.group()) if match else 0

    def _extract_npc_names(self, text):
        all_npcs = set(self.npc_data.keys())
        words = set(re.findall(r'\b\w+\b', text.lower()))
        return [name for name in all_npcs if name.lower() in words and name.lower() != text.lower()]

    def _analyze_sentiment(self, text):
        positive_keywords = ["like", "helped", "nice", "kind", "good", "respect", "smart"]
        negative_keywords = ["hate", "bad", "mean", "cheated", "unfriendly", "stupid"]

        text_lower = text.lower()
        score = 0
        for word in positive_keywords:
            if word in text_lower:
                score += 1
        for word in negative_keywords:
            if word in text_lower:
                score -= 1

        if score > 0:
            return "positive"
        elif score < 0:
            return "negative"
        return "neutral"

    def _update_attitude_and_share_plotka(self, from_npc, to_npc, message, sentiment):
        print(f"\n→ Mentioned NPC '{to_npc}' with sentiment: {sentiment}")
        self.share_info(from_npc, to_npc, f"The main character mentioned you: '{message}'")

        current = self.npc_data[to_npc].get("attitude_towards_player", "neutral")
        new_attitude = self._adjust_attitude(current, sentiment)
        self.npc_data[to_npc]["attitude_towards_player"] = new_attitude

        self._save_npc_to_file(to_npc)
        self._reload_npc_from_file(to_npc)

    def _adjust_attitude(self, current, sentiment):
        mapping = {
            "neutral": {"positive": "positive", "negative": "negative"},
            "positive": {"negative": "neutral"},
            "negative": {"positive": "neutral"}
        }
        return mapping.get(current, {}).get(sentiment, current)

if __name__ == "__main__":
    npc_manager = NPCManager(data_folder="./Data")
    try:
        while not npc_manager.pipe_server.stop_event_write.is_set():
            threading.Event().wait(0.5)
    except KeyboardInterrupt:
        npc_manager.pipe_server.Stop()
