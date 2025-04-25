from fastapi import FastAPI, HTTPException, Header
from pydantic import BaseModel
from transformers import AutoTokenizer, AutoModelForTextToWaveform
import torch
import torchaudio
import io, os
from fastapi.responses import StreamingResponse
import soundfile as sf  # ✅ Use this instead of torchaudio.save
from dotenv import load_dotenv

load_dotenv()

# ✅ Initialize FastAPI app
app = FastAPI()

API_KEY = os.environ.get("VQA__ApiKey")
if not API_KEY:
    raise ValueError("API key not configured")

def verify_api_key(api_key: str = Header(None)):
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Forbidden")

# ✅ Load tokenizer and model
tokenizer = AutoTokenizer.from_pretrained("Baghdad99/english_voice_tts", cache_dir="./models")
model = AutoModelForTextToWaveform.from_pretrained("Baghdad99/english_voice_tts", cache_dir="./models")
model.eval()

# ✅ Device
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model.to(device)

# ✅ TTS Function (with in-memory audio data)
def text_to_speech(text: str):
    inputs = tokenizer(text, return_tensors="pt").to(device)
    with torch.no_grad():
        output = model(**inputs)
    waveform = output["waveform"]

    if isinstance(waveform, torch.Tensor):
        waveform = waveform.detach().cpu()

    # Use BytesIO to store the audio file in memory instead of saving to disk
    audio_bytes = io.BytesIO()
    sf.write(audio_bytes, waveform.T, samplerate=16000, format='WAV')
    audio_bytes.seek(0)  # Rewind the buffer to the beginning
    return audio_bytes

# ✅ Pydantic model to parse input
class TextInput(BaseModel):
    text: str

# ✅ Define an endpoint to generate speech and return it as a response
@app.post("/text-to-speech/")
async def generate_speech(input_text: TextInput, api_key: str = Header(...)):
    try:
        verify_api_key(api_key)

        # Generate the speech file from the input text (in-memory)
        audio_bytes = text_to_speech(input_text.text)
        
        # Return the audio file directly as a StreamingResponse (no need to save it)
        return StreamingResponse(audio_bytes, media_type="audio/wav", headers={"Content-Disposition": "attachment; filename=generated_speech.wav"})
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating speech: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8002)