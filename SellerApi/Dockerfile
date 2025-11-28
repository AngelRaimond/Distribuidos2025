FROM python:3.11-slim

WORKDIR /app

# Install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy application files
COPY . .

# Create generated directory and generate gRPC code from proto files
RUN mkdir -p ./generated && \
    python -m grpc_tools.protoc \
    -I./protos \
    --python_out=./generated \
    --grpc_python_out=./generated \
    ./protos/seller.proto

# Expose gRPC port
EXPOSE 50052

# Run the application
CMD ["python", "main.py"]
