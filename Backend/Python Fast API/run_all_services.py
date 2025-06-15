import subprocess
import time
import os

# Define relative paths to each service script
services = [
    {"name": "VQA", "file": "VQA-blib-base.py", "port": 8000},
    {"name": "Grammar Correction", "file": "GrammarCorrection.py", "port": 8001},
    {"name": "Text to Speech", "file": "text-to-speech.py", "port": 8002},
]

processes = []

try:
    for service in services:
        print(f"Starting {service['name']} on port {service['port']}...")

        # Run each Python file in a subprocess
        p = subprocess.Popen(["python", service["file"]], cwd=os.path.dirname(__file__))
        processes.append(p)

        # Wait a bit before starting the next to avoid clashing startup
        time.sleep(2)

    print("\nAll services started. Press Ctrl+C to stop.")

    # Keep the script alive while subprocesses run
    for p in processes:
        p.wait()

except KeyboardInterrupt:
    print("\nStopping all services...")
    for p in processes:
        p.terminate()
    print("All services terminated.")
