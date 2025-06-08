from langchain_community.document_loaders import PyPDFLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.vectorstores.faiss import FAISS
from langchain.chains import RetrievalQA
from langchain_core.documents import Document
from langchain_openai import ChatOpenAI
from langchain_huggingface import HuggingFaceEmbeddings
from dotenv import load_dotenv
import os


class RAG:
    # def __init__(self, pdf_path):
    def __init__(self, text):
        self.set_npc_data(text)

    def set_npc_data(self, text):
        documents = [Document(page_content=text)]

        #
        # 1. Loading PDF
        # loader = PyPDFLoader(pdf_path)
        # documents = loader.load()

        # 1.1 Loading text

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
        self.qa_chain = RetrievalQA.from_chain_type(
            llm=self.llm,
            retriever=self.retriever,
            return_source_documents=True
        )

    def update_npc_data(self, text):
        self.set_npc_data(text)

    def answer(self, question: str) -> str:
        result = self.qa_chain.invoke(question)

        # Results
        # print("Odpowiedź:")
        answer = result["result"]

        # print(result["result"])
        # print("\nFragmenty źródłowe:")
        # for doc in result["source_documents"]:
        #    print("-", doc.page_content[:100])

        return result, answer


if __name__ == "__main__":
    pdf_path = r"NPC_Rag\Data\Kowal3.pdf"
    rag = RAG(pdf_path)

    question = "są rasy których nie lubisz?"

    result, answer = rag.answer(question)
    print(answer)