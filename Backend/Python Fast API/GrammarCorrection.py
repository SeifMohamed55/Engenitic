import logging
from fastapi import FastAPI, File, Form, HTTPException, Header, UploadFile
import os
import difflib
from pydantic import BaseModel
import requests

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

API_KEY = os.environ.get("VQA__ApiKey")
ACCESS_TOKEN = os.environ.get("GrammarCorrection__AccessToken")

app = FastAPI()

if not API_KEY:
    raise RuntimeError("VQA__ApiKey environment variable not set")

API_URL = "https://api-inference.huggingface.co/models/grammarly/coedit-large"
headers = {
    "Authorization": f"Bearer {ACCESS_TOKEN}"
}

def verify_api_key(api_key: str = Header(...)):
    """Verify the API key"""
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Invalid API key")

# Query the API
def query(input_text):
    payload = {
        "inputs": f"Fix grammatical errors in this sentence: {input_text}"
    }
    response = requests.post(API_URL, headers=headers, json=payload)
    result = response.json()
    # Handle errors
    if isinstance(result, dict) and result.get("error"):
        raise Exception(f"API error: {result['error']}")
    # If response is a list of dicts with 'generated_text'
    if isinstance(result, list) and 'generated_text' in result[0]:
        return result[0]['generated_text']
    return result

# Highlight inserted words
def highlight_changes(original, corrected):
    original_words = original.split()
    corrected_words = corrected.split()
    diff = list(difflib.ndiff(original_words, corrected_words))

    highlighted_text = ""
    for word in diff:
        tag = word[:2]
        content = word[2:]
        if tag == '+ ':
            highlighted_text += f"\033[91m{content}\033[0m "  # Red (inserted)
        elif tag == '- ':
            # Optionally: show deletions as strikethrough (disabled here)
            continue
        elif tag == '  ':
            highlighted_text += content + " "
    return highlighted_text.strip()

# Rate grammar
def rate_grammar(original, corrected):
    original_words = original.split()
    corrected_words = corrected.split()
    diff = difflib.ndiff(original_words, corrected_words)
    insertions = sum(1 for word in diff if word.startswith('+'))
    grammar_score = 100 - (insertions / len(original_words)) * 100
    return round(grammar_score, 2)

class QuestionRequest(BaseModel):
    sentence: str

@app.post("/predict")
async def predict(
    payload: QuestionRequest,
    api_key: str = Header(...)
):
    """Endpoint for visual question answering"""
    verify_api_key(api_key)
    try:
        edited_text = query(payload.sentence)
        grammar_score = rate_grammar(payload.sentence, edited_text)
        return {
            "correctedText": edited_text,
            "score": grammar_score,
        }
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}")
        raise HTTPException(status_code=500, detail="Internal server error")


# === MAIN ===
# User input
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)