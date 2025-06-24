from dotenv import load_dotenv
import os

# Laod env variables
load_dotenv()

# Set env variables for other classes/scripts

# Log
LOG_PATH = os.getenv("LOG_PATH")

# Pipe server
ENUM_CODES_PATH = os.getenv("ENUM_CODES_PATH")
PIPE_NAME_READ = os.getenv("PIPE_NAME_READ")
PIPE_NAME_WRITE = os.getenv("PIPE_NAME_WRITE")
