import os
import json

from dotenv import load_dotenv

from langchain.chains import RetrievalQA, ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory
from langchain.prompts import PromptTemplate
from langchain.schema import Document
from langchain.text_splitter import RecursiveCharacterTextSplitter

from langchain_community.vectorstores.faiss import FAISS
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_openai import ChatOpenAI

from Utils import Log, MessageType as mt

class RAG:
    def __init__(self, json_path):

        with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        text = npc_json_to_text(data)
        doc = Document(page_content=text, metadata={"npc": data.get("name", "unknown")})

        documents = [doc]

        template = """
        Here is what your conversation looks like so far:
        {chat_history}

        Here is a description of you:
        {context}

        Now respond to this message:
        {question}
        """

        prompt = PromptTemplate(
            input_variables=["chat_history", "question", "context"],
            template=template
        )

        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=300,
            chunk_overlap=20,
            length_function=len,
            separators=["\n\n", "\n", " ", ""]
        )

        chunks = text_splitter.split_documents(documents)
        Log(self.class_name, mt.LOG, f"Split document into {len(chunks)} chunks")

        self.embedding_model = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")
        Log(self.class_name, mt.LOG, f"Used embedded mode: sentence-transformers/all-MiniLM-L6-v2")
        
        self.vectorstore = FAISS.from_documents(chunks, self.embedding_model)

        Log(self.class_name, mt.LOG, f"Vector database created successfully")

        self.retriever = self.vectorstore.as_retriever(search_kwargs={"k": 3})
        
        load_dotenv()

        self.llm = ChatOpenAI(
            model_name="gpt-4o-mini",
            openai_api_key=os.environ['OPENAI_API_KEY'],
        )

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
        result = self.qa_chain.invoke({"question": question})
        answer = result["answer"]
        return result, answer

def npc_json_to_text(npc_data: dict) -> str:
    lines = [
        "Below is a description of the character you are:",
        f"Your NAME: {npc_data['name']}",
        f"Your ROLE: {npc_data['role']}\n",
        f"Your DESCRIPTION:\n{npc_data['description']}",
        f"Your attitude towards the player: {npc_data['attitude_towards_player']}\n",
        "ITEMS you have FOR SALE:",
    ]

    for item in npc_data['items']:
        lines.append(f"- {item['name']} – {item['price']}")

    lines.extend([
        "\nRELATIONSHIPS you have with other residents:",
        f"- Likes: {', '.join(npc_data['relations']['likes'])}",
        f"- Dislikes: {', '.join(npc_data['relations']['dislikes'])}\n",
        "RUMORS circulating about you:",
    ])

    for plotka in npc_data['rumors']:
        lines.append(f"- {plotka}")

    lines.append(f"\nCURRENCY used in your world: {npc_data['currency']}")

    return "\n".join(lines)