import os
import re
import json
from langchain.chains import RetrievalQA
from dotenv import load_dotenv
from langchain_core.documents import Document

from NPC_Rag.NPC_Rag import RAG

class NPCManager:
    def __init__(self, data_folder):
        self.data_folder = data_folder
        self.npc_data = {}
        self.npc_agents = {}

        load_dotenv()
        self.load_all_npcs()

    def load_all_npcs(self):
        for file in os.listdir(self.data_folder):
            if file.endswith(".json"):
                npc_name = file[:-5]
                path = os.path.join(self.data_folder, file)
                with open(path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                self.npc_data[npc_name] = data
                text = self._flatten_npc_to_text(data)
                self.npc_agents[npc_name] = RAG(text)

    def _flatten_npc_to_text(self, data):
        parts = [
            f"{data['imie']} - {data['rola']}",
            data["opis"],
            f"Nastawienie do gracza: {data.get('nastawienie_do_gracza', 'neutralne')}",
            "Przedmioty w ofercie:" + ''.join(f"\n- {p['nazwa']} ({p['cena']})" for p in data["przedmioty"]),
            f"Relacje:\n  Lubi: {', '.join(data['relacje'].get('lubi', []))}\n  Nie lubi: {', '.join(data['relacje'].get('nie_lubi', []))}",
            "Plotki:" + ''.join(f"\n- {p}" for p in data["plotki"]),
            f"Waluta: {data['waluta']}"
        ]
        return "\n".join(parts)

    def share_info(self, from_npc, to_npc, message):
        if "plotki" not in self.npc_data[to_npc]:
            self.npc_data[to_npc]["plotki"] = []

        self.npc_data[to_npc]["plotki"].append(f"[Plotka od {from_npc}]: {message}")
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
        self.npc_agents[npc_name].update_npc_data(data)

    def talk_to_npc(self, npc_name, text):
        self._reload_npc_from_file(npc_name)

        agent = self.npc_agents.get(npc_name)
        if not agent:
            print(f"NPC '{npc_name}' nie istnieje lub nie został załadowany.")
            return

        result, answer = agent.answer(text)
        print("Odpowiedź NPC-a:")
        print(answer)
        print("\nŹródła odpowiedzi:")
        for doc in result["source_documents"]:
            print("-", doc.page_content[:100])

        mentioned_npcs = self._extract_npc_names(text)
        for mentioned in mentioned_npcs:
            if mentioned in self.npc_data:
                sentiment = self._analyze_sentiment(text)
                self._update_attitude_and_share_plotka(npc_name, mentioned, text, sentiment)

    def _extract_npc_names(self, text):
        all_npcs = set(self.npc_data.keys())
        words = set(re.findall(r'\b\w+\b', text.lower()))
        return [name for name in all_npcs if name.lower() in words and name.lower() != text.lower()]

    def _analyze_sentiment(self, text):
        positive_keywords = ["lubię", "pomógł", "fajny", "miły", "dobry", "szanuję"]
        negative_keywords = ["nienawidzę", "zły", "wredny", "oszukał", "niefajny"]

        text_lower = text.lower()
        score = 0
        for word in positive_keywords:
            if word in text_lower:
                score += 1
        for word in negative_keywords:
            if word in text_lower:
                score -= 1

        if score > 0:
            return "pozytywne"
        elif score < 0:
            return "negatywne"
        return "neutralne"

    def _update_attitude_and_share_plotka(self, from_npc, to_npc, message, sentiment):
        print(f"\n→ Wspomniano o NPC '{to_npc}' z nastawieniem: {sentiment}")

        self.share_info(from_npc, to_npc, f"{from_npc} wspomniał o Tobie: '{message}'")

        current = self.npc_data[to_npc].get("nastawienie_do_gracza", "neutralne")
        new_attitude = self._adjust_attitude(current, sentiment)
        self.npc_data[to_npc]["nastawienie_do_gracza"] = new_attitude

        self._save_npc_to_file(to_npc)
        self.npc_agents[to_npc].update_npc_data(self.npc_data[to_npc])

    def _adjust_attitude(self, current, sentiment):
        mapping = {
            "neutralne": {"pozytywne": "pozytywne", "negatywne": "negatywne"},
            "pozytywne": {"negatywne": "neutralne"},
            "negatywne": {"pozytywne": "neutralne"}
        }
        return mapping.get(current, {}).get(sentiment, current)