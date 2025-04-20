from fastapi import FastAPI, Form, UploadFile, File
from transformers import AutoProcessor, AutoModelForVisualQuestionAnswering
from PIL import Image
from io import BytesIO
import os
from fastapi import FastAPI, Header, HTTPException

app = FastAPI()

# Load the BLIP-VQA model
processor = AutoProcessor.from_pretrained("Salesforce/blip-vqa-base", cache_dir="./models")
model = AutoModelForVisualQuestionAnswering.from_pretrained("Salesforce/blip-vqa-base", cache_dir="./models")

API_KEY = os.environ.get("VQA__ApiKey")
if not API_KEY:
    raise ValueError("API key not configured")

def verify_api_key(api_key: str = Header(None)):
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Forbidden")

@app.post("/predict")
async def predict(
        image: UploadFile = File(...), question: str = Form(...), api_key: str = Header(...)):
    try:
        verify_api_key(api_key)

        image = Image.open(BytesIO(await image.read())).convert("RGB")
        
        # Process input
        inputs = processor(images=image, text=question, return_tensors="pt")
        
        # Run inference
        outputs = model.generate(**inputs)
        
        # Decode result
        answer = processor.batch_decode(outputs, skip_special_tokens=True)[0]
        
        return {"answer": answer}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=f"Prediction error: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

# uvicorn main:app --host 0.0.0.0 --port 8000 // run command