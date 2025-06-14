# FastAPI.Dockerfile

FROM python:3.10-slim

# Set working directory inside container
WORKDIR /app

# Install dependencies
COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

# Copy all FastAPI service files into the container
COPY . /app

# Expose all required ports (optional but helpful for documentation)
EXPOSE 8000 8001 8002

# Default command, will be overridden in docker-compose
CMD ["python", "main.py"]
