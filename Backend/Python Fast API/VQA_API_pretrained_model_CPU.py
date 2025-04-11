from transformers import Blip2Processor, Blip2ForConditionalGeneration
import torch

# Define a local directory to save the model
local_path = "./blip2-opt-2.7b"  

# Download and save the model locally
processor = Blip2Processor.from_pretrained("Salesforce/blip2-opt-2.7b")
processor.save_pretrained(local_path)

model = Blip2ForConditionalGeneration.from_pretrained("Salesforce/blip2-opt-2.7b")
model.save_pretrained(local_path)

print("Model and processor saved locally at:", local_path)

# Load the processor and model from the local directory
processor = Blip2Processor.from_pretrained(local_path)
model = Blip2ForConditionalGeneration.from_pretrained(local_path)

# Move model to GPU if available
device = "cpu"
model.to(device)

# Ensure model is in evaluation mode
model.eval()

from fastapi import FastAPI, UploadFile, File, Form
from PIL import Image
import io

app = FastAPI()

# Function to process image and question
def predict_answer(image: Image.Image, question: str):
    inputs = processor(images=image, text=question, return_tensors="pt")
    with torch.no_grad():
        outputs = model.generate(**inputs)
    answer = processor.tokenizer.decode(outputs[0], skip_special_tokens=True)
    return answer

@app.post("/vqa/")
async def vqa(file: UploadFile = File(...), question: str = Form(...)):
    """Accepts an image and a question, returns an answer."""
    # Read the image
    contents = await file.read()
    image = Image.open(io.BytesIO(contents)).convert("RGB")

    # Get prediction
    answer = predict_answer(image, question)

    return {"question": question, "answer": answer}