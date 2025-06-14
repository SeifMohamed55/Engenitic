import subprocess
import time

# Define the commands to run each app
services = [
    {"name": "VQA", "file": "Backend/Python Fast API/VQA-blib-base.py", "port": 8000},
    {"name": "Grammar Correction", "file": "Backend/Python Fast API/GrammarCorrection.py", "port": 8001},
    {"name": "Text-to-Speech", "file": "Backend/Python Fast API/text-to-speech.py", "port": 8002},
]

processes = []

try:
    for service in services:
        print(f"Starting {service['name']} on port {service['port']}...")
        p = subprocess.Popen(["python", service["file"]])
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
