import json
from Utils import Log, MessageType as mt
import re
from langsmith import traceable
from Pipe import Item

class IntentInterpreter:

    class_name = "INTENT INTERPRETER"

    def __init__(self, llm_agent):
        self.agent = llm_agent

        # get items list
        self.items:str = ""
        for item in Item.__members__:
            self.items += item + "/"
        self.items = self.items[:-1]

    @traceable(name="Analising message intent")
    def interpret(self, message):

        #todo move some parts to config
        prompt = (
            f"""Analyze the player's message and return JSON with their intent.

            Message: "{message}"

            Return JSON like:
            {{
                "intent": "buy/sell/talk/insult/praise/unknown",
                "target_npc": "HEARBALIST/SMITH/BAKER or null",
                "item": "{self.items}",
                "quantity": number,
                "sentiment": "positive/negative/neutral"
            }}

            Remember:
            - If the player compliments the NPC, it's "praise" with "positive" sentiment.
            - If they insult them, it's "insult" with "negative" sentiment.
            - If they try to buy/sell, include item and quantity but if they dont then set it to 0.
            """
        )
        _, raw_response = self.agent.answer(prompt)

        try:
            # get llm response json
            response_json_str = re.search(r'```json\s*(\{.*?\})\s*```', raw_response, re.DOTALL).group(1)
            response_json = json.loads(response_json_str)

            # construct log message
            log_response = "Analisis result: ("
            log_response += f"Intent: {response_json["intent"]} "
            log_response += f"Target NPC: {response_json["target_npc"]} "
            log_response += f"Item: {response_json["item"]} "
            log_response += f"Quantity: {response_json["quantity"]} "
            log_response += f"Sentiment: {response_json["sentiment"]})"
            Log(self.class_name, mt.LOG, log_response)

            # return analisis result
            return response_json

        except:
            Log(self.class_name, mt.ERROR, "Error while analising message")
            return {
                "intent": "unknown",
                "target_npc": None,
                "item": None,
                "quantity": 1,
                "sentiment": "neutral"
            }
