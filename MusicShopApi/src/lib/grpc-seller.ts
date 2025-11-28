import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Load proto file
const PROTO_PATH = path.join(__dirname, '../../protos/seller.proto');

const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true
});

const sellerProto = grpc.loadPackageDefinition(packageDefinition).seller as any;

// gRPC client configuration
const GRPC_HOST = process.env.SELLER_GRPC_HOST || 'localhost';
const GRPC_PORT = process.env.SELLER_GRPC_PORT || '50052';
const GRPC_ADDRESS = `${GRPC_HOST}:${GRPC_PORT}`;

// Create gRPC client
const client = new sellerProto.SellerService(
  GRPC_ADDRESS,
  grpc.credentials.createInsecure()
);

export interface Sale {
  instrument_name: string;
  amount: number;
  sale_date: string;
}

export interface CreateSellerRequest {
  name: string;
  email: string;
  age: number;
  hire_date: string;
  sales: Sale[];
  phone: string;
  address: string;
}

export interface SellerResponse {
  id: string;
  name: string;
  email: string;
  age: number;
  hire_date: string;
  sales: Sale[];
  phone: string;
  address: string;
  total_sales: number;
  created_at: string;
  updated_at: string;
}

export interface CreateSellerResponse {
  sellers_created: number;
  seller_ids: string[];
  message: string;
}

export interface UpdateSellerRequest {
  id: string;
  name?: string;
  email?: string;
  age?: number;
  hire_date?: string;
  sales?: Sale[];
  phone?: string;
  address?: string;
}

export interface DeleteSellerResponse {
  success: boolean;
  message: string;
}

export class SellerGrpcClient {
  /**
   * Create sellers using client streaming
   * Sends multiple seller requests and receives a single response
   */
  static createSellers(sellers: CreateSellerRequest[]): Promise<CreateSellerResponse> {
    return new Promise((resolve, reject) => {
      const call = client.CreateSeller((error: any, response: any) => {
        if (error) {
          reject(error);
        } else {
          resolve(response);
        }
      });

      // Stream each seller to the server
      sellers.forEach(seller => {
        call.write(seller);
      });

      // End the stream
      call.end();
    });
  }

  /**
   * Get sellers by name using server streaming
   * Sends a single request and receives multiple seller responses
   */
  static getSellersByName(name: string): Promise<SellerResponse[]> {
    return new Promise((resolve, reject) => {
      const sellers: SellerResponse[] = [];

      const call = client.GetSellerByName({ name });

      call.on('data', (seller: SellerResponse) => {
        sellers.push(seller);
      });

      call.on('end', () => {
        resolve(sellers);
      });

      call.on('error', (error: any) => {
        reject(error);
      });
    });
  }

  /**
   * Get a seller by ID (unary)
   */
  static getSellerById(id: string): Promise<SellerResponse> {
    return new Promise((resolve, reject) => {
      client.GetSellerById({ id }, (error: any, response: SellerResponse) => {
        if (error) {
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }

  /**
   * Update a seller (unary)
   */
  static updateSeller(updateData: UpdateSellerRequest): Promise<SellerResponse> {
    return new Promise((resolve, reject) => {
      client.UpdateSeller(updateData, (error: any, response: SellerResponse) => {
        if (error) {
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }

  /**
   * Delete a seller (unary)
   */
  static deleteSeller(id: string): Promise<DeleteSellerResponse> {
    return new Promise((resolve, reject) => {
      client.DeleteSeller({ id }, (error: any, response: DeleteSellerResponse) => {
        if (error) {
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }

  /**
   * Check if gRPC service is available
   */
  static async healthCheck(): Promise<boolean> {
    return new Promise((resolve) => {
      const deadline = new Date();
      deadline.setSeconds(deadline.getSeconds() + 5);

      client.waitForReady(deadline, (error: any) => {
        if (error) {
          console.error('SellerApi gRPC service not ready:', error.message);
          resolve(false);
        } else {
          console.log('SellerApi gRPC service is ready');
          resolve(true);
        }
      });
    });
  }
}

export default SellerGrpcClient;
