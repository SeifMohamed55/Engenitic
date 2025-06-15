import logging
from fastapi import FastAPI, File, Form, HTTPException, Header, UploadFile
import os
import difflib
from pydantic import BaseModel
import requests
from dotenv import load_dotenv
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM
import difflib
import re

model_name = "grammarly/coedit-large"

# Load the tokenizer and model from the local path
tokenizer = AutoTokenizer.from_pretrained(model_name, cache_dir="./models")
model = AutoModelForSeq2SeqLM.from_pretrained(model_name, cache_dir="./models")


load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

API_KEY = os.environ.get("VQA__ApiKey")


app = FastAPI()

def verify_api_key(api_key: str = Header(...)):
    """Verify the API key"""
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Invalid API key")

# Query
def query(input_text: str) -> str:
    prompt = f"Fix grammatical errors: {input_text}"
    inputs = tokenizer(prompt, return_tensors="pt", truncation=True)
    outputs = model.generate(**inputs, max_length=256)
    corrected_text = tokenizer.decode(outputs[0], skip_special_tokens=True)
    return corrected_text


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

def normalize_word(word):
    """Lowercase and remove simple punctuation."""
    return re.sub(r'[^\w\s]', '', word).lower()

def rate_grammar(original, corrected):
    original_words = [normalize_word(w) for w in original.split()]
    corrected_words = [normalize_word(w) for w in corrected.split()]

    diff = difflib.ndiff(original_words, corrected_words)
    insertions = sum(1 for word in diff if word.startswith('+ '))

    # Prevent division by zero
    if not original_words:
        return 100.0 if not corrected_words else 0.0

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
