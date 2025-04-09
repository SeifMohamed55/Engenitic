from fastapi import FastAPI, Form, UploadFile, File
from transformers import AutoProcessor, AutoModelForVisualQuestionAnswering
from PIL import Image
from io import BytesIO
import os
from fastapi import FastAPI, Header, HTTPException
from transformers import Blip2Processor, Blip2ForConditionalGeneration
import torch

app = FastAPI()
# Load the BLIP-VQA model
# processor = AutoProcessor.from_pretrained("Salesforce/blip-vqa-base", cache_dir="./models")
# model = AutoModelForVisualQuestionAnswering.from_pretrained("Salesforce/blip-vqa-base", cache_dir="./models")

# Define a local directory to save the model
local_path = "./models/blip2-opt-2.7b"  

# Load the processor and model from the local directory
processor = Blip2Processor.from_pretrained(local_path)
model = Blip2ForConditionalGeneration.from_pretrained(local_path)

# Move model to GPU if available
device = "cpu"
model.to(device)

# Ensure model is in evaluation mode
model.eval()

API_KEY = os.environ.get("VQA__ApiKey")
if not API_KEY:
    raise ValueError("API key not configured")

def verify_api_key(api_key: str = Header(None)):
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Forbidden")
    
def predict_answer(image: Image.Image, question: str):
    inputs = processor(images=image, text=question, return_tensors="pt")
    with torch.no_grad():
        outputs = model.generate(**inputs)
    answer = processor.tokenizer.decode(outputs[0], skip_special_tokens=True)
    if not answer:
        raise HTTPException(status_code=400, detail="No answer found")
    return answer

@app.post("/predict")
async def predict(
        image: UploadFile = File(...), question: str = Form(...), api_key: str = Header(...)):
    try:
        verify_api_key(api_key)

        image = Image.open(BytesIO(await image.read())).convert("RGB")
        
        # # Process input
        # inputs = processor(images=image, text=question, return_tensors="pt")
        
        # # Run inference
        # outputs = model.generate(**inputs)
        
        # # Decode result
        # answer = processor.batch_decode(outputs, skip_special_tokens=True)[0]
        
        answer = predict_answer(image, question)

        return {"answer": answer}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=f"Prediction error: {str(e)}")

# uvicorn main:app --host 0.0.0.0 --port 8000 // run command