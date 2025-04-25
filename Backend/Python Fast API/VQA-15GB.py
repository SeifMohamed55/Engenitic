from fastapi import FastAPI, Form, UploadFile, File, Header, HTTPException
from transformers import Blip2Processor, Blip2ForConditionalGeneration
from PIL import Image
from io import BytesIO
import os
import torch
import logging
from dotenv import load_dotenv

load_dotenv()

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

# Device configuration
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
logger.info(f"Using device: {device}")

# Model configuration
MODEL_PATH = "./blip2-opt-2.7b"
API_KEY = os.environ.get("VQA__ApiKey")

# Validate environment
if not API_KEY:
    raise RuntimeError("VQA__ApiKey environment variable not set")

# Load model with error handling
try:
    logger.info("Loading model...")
    processor = Blip2Processor.from_pretrained(MODEL_PATH)
    model = Blip2ForConditionalGeneration.from_pretrained(
        MODEL_PATH,
        torch_dtype=torch.float16 if device.type == "cuda" else torch.float32
    ).to(device)
    model.eval()
    logger.info("Model loaded successfully")
except Exception as e:
    logger.error(f"Model loading failed: {str(e)}")
    raise

def verify_api_key(api_key: str = Header(...)):
    """Verify the API key"""
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Invalid API key")

def predict_answer(image: Image.Image, question: str) -> str:
    """Generate answer for the given image and question"""
    try:
        # Process inputs and ensure device consistency
        inputs = processor(
            images=image,
            text=question,
            return_tensors="pt"
        ).to(device)
        
        # Double-check model device
        model.to(device)
        
        # Generate answer
        with torch.no_grad():
            outputs = model.generate(**inputs)
        
        # Decode and clean answer
        answer = processor.decode(
            outputs[0],
            skip_special_tokens=True
        ).replace(question, "").replace('?', '').strip()
        
        return answer if answer else "No answer generated"
    
    except RuntimeError as e:
        logger.error(f"Prediction error: {str(e)}")
        if "CUDA out of memory" in str(e):
            torch.cuda.empty_cache()
            raise HTTPException(
                status_code=500,
                detail="GPU memory exhausted. Try with a smaller image or question."
            )
        raise HTTPException(status_code=500, detail="Prediction failed")

@app.post("/predict")
async def predict(
    image: UploadFile = File(..., description="Image file"),
    question: str = Form(..., description="Question about the image"),
    api_key: str = Header(..., description="API key for authentication")
):
    """Endpoint for visual question answering"""
    verify_api_key(api_key)
    
    try:
        # Read and validate image
        image_data = await image.read()
        if not image_data:
            raise HTTPException(status_code=400, detail="Empty image file")
        
        img = Image.open(BytesIO(image_data)).convert("RGB")
        
        # Get prediction
        answer = predict_answer(img, question)
        
        return {
            "question": question,
            "answer": answer,
            "device": str(device)
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}")
        raise HTTPException(status_code=500, detail="Internal server error")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)