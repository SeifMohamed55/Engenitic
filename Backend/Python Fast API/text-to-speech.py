# ✅ Imports
from transformers import AutoTokenizer, AutoModelForTextToWaveform
import torchaudio
import torch
import gradio as gr
import random
import numpy as np

# ✅ Reproducibility
torch.manual_seed(42)
np.random.seed(42)
random.seed(42)

# ✅ Load tokenizer and model
tokenizer = AutoTokenizer.from_pretrained("Baghdad99/english_voice_tts")
model = AutoModelForTextToWaveform.from_pretrained("Baghdad99/english_voice_tts")
model.eval()

# ✅ Device
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model.to(device)

# ✅ TTS Function for API
def text_to_speech(text):
    inputs = tokenizer(text, return_tensors="pt").to(device)
    with torch.no_grad():
        output = model(**inputs)
    waveform = output["waveform"]

    if isinstance(waveform, torch.Tensor):
        waveform = waveform.detach().cpu()

    # Save as temporary file
    file_path = "output_tts.wav"
    torchaudio.save(file_path, waveform, sample_rate=16000)
    return file_path

# ✅ Gradio Interface
iface = gr.Interface(
    fn=text_to_speech,
    inputs=gr.Textbox(label="Enter text here"),
    outputs=gr.Audio(type="filepath", label="Generated Speech"),
    title="English Text-to-Speech",
    description="Enter any English text and get the generated voice using Baghdad99/english_voice_tts model."
)

# ✅ Launch the API
iface.launch(debug=True)





