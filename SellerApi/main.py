import grpc
from concurrent import futures
import os
import sys
import time
from dotenv import load_dotenv

load_dotenv()

# Agrega directorio 'generated' al path para acceder a archivos protobuf compilados
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'generated'))

import seller_pb2_grpc
from services.seller_service import SellerService


def serve():
    """Inicia el servidor gRPC con ThreadPoolExecutor (10 workers) y lo mantiene corriendo.
    Escucha en todas las interfaces (::) en el puerto configurado (default: 50052)."""
    port = os.getenv('GRPC_PORT', '50052')
    
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    seller_pb2_grpc.add_SellerServiceServicer_to_server(SellerService(), server)
    server.add_insecure_port(f'[::]:{port}')
    
    server.start()
    print(f'SellerApi gRPC server started on port {port}')
    
    try:
        while True:
            time.sleep(86400)
    except KeyboardInterrupt:
        print('Shutting down gRPC server...')
        server.stop(0)


if __name__ == '__main__':
    serve()
