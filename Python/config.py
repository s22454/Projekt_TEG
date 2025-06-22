from dotenv import load_dotenv
import os

# laod env variables
load_dotenv()

# set env variables for other classes/scripts
ENUM_CODES_PATH = os.getenv("ENUM_CODES_PATH")
PIPE_NAME_READ = os.getenv("PIPE_NAME_READ")
PIPE_NAME_WRITE = os.getenv("PIPE_NAME_WRITE")
