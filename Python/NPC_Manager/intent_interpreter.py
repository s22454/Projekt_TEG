class IntentInterpreter:
    def __init__(self, llm_agent):
        self.agent = llm_agent

    def interpret(self, message):
        prompt = (
            f"""Analyze the player's message and return JSON with their intent.

            Message: "{message}"

            Return JSON like:
            {{
                "intent": "buy/sell/talk/insult/praise/unknown",
                "target_npc": "name or null",
                "item": "name or null",
                "quantity": number,
                "sentiment": "positive/negative/neutral"
            }}

            Remember:
            - If the player compliments the NPC, it's "praise" with "positive" sentiment.
            - If they insult them, it's "insult" with "negative" sentiment.
            - If they try to buy/sell, include item and quantity.
            """
        )
        _, raw_response = self.agent.answer(prompt)

        try:
            import json
            return json.loads(raw_response)
        except:
            return {
                "intent": "unknown",
                "target_npc": None,
                "item": None,
                "quantity": 1,
                "sentiment": "neutral"
            }