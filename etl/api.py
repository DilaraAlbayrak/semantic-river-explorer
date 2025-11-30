from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer

app = Flask(__name__)

# Load model ONCE when the script starts (This takes time only at startup)
print("Loading AI Model into Memory...")
model = SentenceTransformer('all-MiniLM-L6-v2')
print("Model Loaded! API is ready.")

@app.route('/embed', methods=['POST'])
def embed():
    data = request.json
    text = data.get('text', '')
    
    if not text:
        return jsonify({'error': 'No text provided'}), 400

    # Generate vector (Takes milliseconds because model is already loaded)
    embedding = model.encode(text).tolist()
    
    return jsonify(embedding)

if __name__ == '__main__':
    # Runs on http://localhost:5000
    app.run(port=5000)