import subprocess
import time
import os

# Set the path to your FastAPI scripts
BASE_DIR = os.path.join(os.getcwd(), "Backend", "Python Fast API")

services = [
    {"name": "VQA", "file": "VQA-blib-base.py", "port": 8000},
    {"name": "Grammar Correction", "file": "GrammarCorrection.py", "port": 8001},
    {"name": "Text-to-Speech", "file": "text-to-speech.py", "port": 8002},
]

processes = []

try:
    for service in services:
        print(f"Starting {service['name']} on port {service['port']}...")
        script_path = os.path.join(BASE_DIR, service["file"])
        p = subprocess.Popen(["python", script_path], cwd=BASE_DIR)
        processes.append(p)
        time.sleep(2)  # slight delay to stagger startup

    print("All services started. Press Ctrl+C to stop.")

    # Keep the script alive
    for p in processes:
        p.wait()

except KeyboardInterrupt:
    print("Shutting down services...")
    for p in processes:
        p.terminate()
    print("All services terminated.")
