import sys
import json
from sentence_transformers import SentenceTransformer

# Suppress warnings to keep output clean for C#
import warnings
warnings.filterwarnings("ignore")

def get_embedding(text):
    # Load the same model used for generating data
    model = SentenceTransformer('all-MiniLM-L6-v2')
    # Generate embedding
    embedding = model.encode(text).tolist()
    return embedding

if __name__ == "__main__":
    # Read input text from command line arguments
    if len(sys.argv) > 1:
        input_text = sys.argv[1]
        vector = get_embedding(input_text)
        # Print only the JSON list so C# can parse it
        print(json.dumps(vector))