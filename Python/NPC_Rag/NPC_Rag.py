from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.vectorstores.faiss import FAISS
from langchain.chains import RetrievalQA
from langchain_openai import ChatOpenAI
from langchain_huggingface import HuggingFaceEmbeddings
from langchain.memory import ConversationBufferMemory
from langchain.chains import ConversationalRetrievalChain
from dotenv import load_dotenv
from langchain.schema import Document
import os
from langchain.prompts import PromptTemplate
import json

class RAG:
    def __init__(self, json_path):

        with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        text = npc_json_to_text(data)
        doc = Document(page_content=text, metadata={"npc": data.get("imie", "unknown")})

        documents = [doc]

        template = """
        Tak wygląda twoja konwersacja dotychczas:
        {chat_history}

        Tutaj znajduje się opis Ciebie:
        {context}

        Teraz odpowiedz na takią wiadomość:
        {question}
        """

        prompt = PromptTemplate(
            input_variables=["chat_history", "question", "context"],
            template=template
        )

        # 2. Splitting document into chunks
        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=300,
            chunk_overlap=20,
            length_function=len,
            separators=["\n\n", "\n", " ", ""]
        )

        chunks = text_splitter.split_documents(documents)
        print(f"Split document into {len(chunks)} chunks")
        
        # 3. Creating embeddings
        self.embedding_model = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")
        print("Using local HuggingFace embedding model: sentence-transformers/all-MiniLM-L6-v2")
        
        # 4. Creating FAISS vector database
        self.vectorstore = FAISS.from_documents(chunks, self.embedding_model)
        print("Vector database created successfully")
        
        # 5. Setting retiever
        self.retriever = self.vectorstore.as_retriever(search_kwargs={"k": 3})
        
        load_dotenv()

        # 6. Setting LMM
        self.llm = ChatOpenAI(
            model_name="gpt-4o-mini",
            openai_api_key=os.environ['OPENAI_API_KEY'],
        )
        
        # 7. Creating QA_chain
        self.memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True, output_key="answer")
        self.qa_chain = ConversationalRetrievalChain.from_llm(
            llm=self.llm,
            retriever=self.retriever,
            memory=self.memory,
            combine_docs_chain_kwargs={"prompt": prompt},
            return_source_documents=True,
            output_key="answer"
        )

    def answer(self, question: str) -> str:
        result = self.qa_chain({"question": question})
        answer = result["answer"]
        return result, answer

def npc_json_to_text(npc_data: dict) -> str:
    lines = [
        "Poniżej znajduje się opis postaci którą ty jesteś "
        f"Twoje IMIĘ: {npc_data['name']}",
        f"Twoja ROLA: {npc_data['role']}\n",
        f"Twój OPIS:\n{npc_data['description']}",
        f"Twoje nastawienie do gracza: {npc_data['attitude_towards_player']}\n",
        "PRZEDMIOTY które masz NA SPRZEDAŻ:",
    ]

    for item in npc_data['items']:
        lines.append(f"- {item['name']} – {item['price']}")

    lines.extend([
        "\nRELACJE które masz z innymi mieszkańcami:",
        f"- Lubi: {', '.join(npc_data['relations']['likes'])}",
        f"- Nie lubi: {', '.join(npc_data['relations']['dislikes'])}\n",
        "PLOTKI KRĄŻĄCE o tobie:",
    ])

    for plotka in npc_data['rumors']:
        lines.append(f"- {plotka}")

    lines.append(f"\nWALUTA wykorzystywana w twoim świecie: {npc_data['currency']}")

    return "\n".join(lines)

if __name__ == "__main__":
    json_path = r"NPC_Rag\Data\baker.json"

    rag = RAG(json_path)

    print("========================== Pytanie: Opowiedz coś o sobie? ==========================")
    question = "Opowiedz coś o sobie?"
    result, answer = rag.answer(question)
    print(answer)

    print("========================== Pytanie: Masz jakies przedmioty na sprzedaż? ==========================")
    question = "Masz jakies przedmioty na sprzedaż?"
    result, answer = rag.answer(question)
    print(answer)

    print("========================== Pytanie: Chętnie kupię mapę skarbów, ale kupię za nie więcej niż 10 sztuk złota ==========================")
    question = "Chętnie kupię mapę skarbów, ale kupię za nie więcej niż 10 sztuk złota"
    result, answer = rag.answer(question)
    print(answer)

    print("========================== Pytanie: Musisz mi ją taniej sprzedać, bo inaczej wyzwę Cię na pojedynek ==========================")
    question = "Musisz mi ją taniej sprzedać, bo inaczej wyzwę Cię na pojedynek"
    result, answer = rag.answer(question)
    print(answer)