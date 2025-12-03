import { Router, Request, Response } from 'express';
import SellerGrpcClient from '../lib/grpc-seller.js';
import { StatusCodes } from '../types.js';

const router = Router();

/**
 * @route POST /api/sellers
 * @desc Create one or multiple sellers using client streaming gRPC
 * @access Public
 * 
 * Request Body:
 * {
 *   "sellers": [
 *     {
 *       "name": "John Doe",
 *       "email": "john@example.com",
 *       "age": 25,
 *       "hire_date": "2024-01-15",
 *       "phone": "+1234567890",
 *       "address": "123 Main St",
 *       "sales": [
 *         {
 *           "instrument_name": "Guitar",
 *           "amount": 500.00,
 *           "sale_date": "2024-11-01"
 *         }
 *       ]
 *     }
 *   ]
 * }
 * 
 * GRPC CALL LOCATION: This endpoint invokes the CreateSeller client streaming RPC
 * See: src/lib/grpc-seller.ts - SellerGrpcClient.createSellers()
 */
router.post('/', async (req: Request, res: Response) => {
  try {
    const { sellers } = req.body;

    if (!sellers || !Array.isArray(sellers) || sellers.length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'sellers array is required and must contain at least one seller'
      });
    }

    // Call gRPC client streaming method
    // *** GRPC CLIENT STREAMING CALL HERE ***
    const result = await SellerGrpcClient.createSellers(sellers);

    return res.status(StatusCodes.CREATED).json({
      success: true,
      data: result
    });

  } catch (error: any) {
    console.error('Error creating sellers:', error);
    
    // Map gRPC error codes to HTTP status codes
    const statusCode = mapGrpcErrorToHttp(error);
    
    return res.status(statusCode).json({
      error: 'Failed to create sellers',
      message: error.message || 'Unknown error',
      details: error.details || error.message
    });
  }
});

/**
 * @route GET /api/sellers/search/:name
 * @desc Search sellers by name using server streaming gRPC
 * @access Public
 * 
 * GRPC CALL LOCATION: This endpoint invokes the GetSellerByName server streaming RPC
 * See: src/lib/grpc-seller.ts - SellerGrpcClient.getSellersByName()
 */
router.get('/search/:name', async (req: Request, res: Response) => {
  try {
    const { name } = req.params;

    if (!name || name.trim().length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'Name parameter is required'
      });
    }

    // Call gRPC server streaming method
    // *** GRPC SERVER STREAMING CALL HERE ***
    const sellers = await SellerGrpcClient.getSellersByName(name);

    return res.status(StatusCodes.OK).json({
      success: true,
      count: sellers.length,
      data: sellers
    });

  } catch (error: any) {
    console.error('Error searching sellers:', error);
    
    const statusCode = mapGrpcErrorToHttp(error);
    
    return res.status(statusCode).json({
      error: 'Failed to search sellers',
      message: error.message || 'Unknown error',
      details: error.details || error.message
    });
  }
});

/**
 * @route GET /api/sellers/:id
 * @desc Get a seller by ID using unary gRPC
 * @access Public
 * 
 * GRPC CALL LOCATION: This endpoint invokes the GetSellerById unary RPC
 * See: src/lib/grpc-seller.ts - SellerGrpcClient.getSellerById()
 */
router.get('/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;

    if (!id || id.trim().length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'Seller ID is required'
      });
    }

    // Call gRPC unary method
    // *** GRPC UNARY CALL HERE ***
    const seller = await SellerGrpcClient.getSellerById(id);

    return res.status(StatusCodes.OK).json({
      success: true,
      data: seller
    });

  } catch (error: any) {
    console.error('Error getting seller:', error);
    
    const statusCode = mapGrpcErrorToHttp(error);
    
    return res.status(statusCode).json({
      error: 'Failed to get seller',
      message: error.message || 'Unknown error',
      details: error.details || error.message
    });
  }
});

/**
 * @route PUT /api/sellers/:id
 * @desc Update a seller using unary gRPC
 * @access Public
 * 
 * Request Body (all fields optional except id):
 * {
 *   "name": "Jane Doe",
 *   "email": "jane@example.com",
 *   "age": 30,
 *   "hire_date": "2024-02-20",
 *   "phone": "+0987654321",
 *   "address": "456 Oak Ave",
 *   "sales": [...]
 * }
 * 
 * GRPC CALL LOCATION: This endpoint invokes the UpdateSeller unary RPC
 * See: src/lib/grpc-seller.ts - SellerGrpcClient.updateSeller()
 */
router.put('/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const updateData = req.body;

    if (!id || id.trim().length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'Seller ID is required'
      });
    }

    if (!updateData || Object.keys(updateData).length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'At least one field must be provided for update'
      });
    }

    // Call gRPC unary method
    // *** GRPC UNARY CALL HERE ***
    const updatedSeller = await SellerGrpcClient.updateSeller({
      id,
      ...updateData
    });

    return res.status(StatusCodes.OK).json({
      success: true,
      data: updatedSeller
    });

  } catch (error: any) {
    console.error('Error updating seller:', error);
    
    const statusCode = mapGrpcErrorToHttp(error);
    
    return res.status(statusCode).json({
      error: 'Failed to update seller',
      message: error.message || 'Unknown error',
      details: error.details || error.message
    });
  }
});

/**
 * @route DELETE /api/sellers/:id
 * @desc Delete a seller using unary gRPC
 * @access Public
 * 
 * GRPC CALL LOCATION: This endpoint invokes the DeleteSeller unary RPC
 * See: src/lib/grpc-seller.ts - SellerGrpcClient.deleteSeller()
 */
router.delete('/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;

    if (!id || id.trim().length === 0) {
      return res.status(StatusCodes.BAD_REQUEST).json({
        error: 'Invalid request',
        message: 'Seller ID is required'
      });
    }

    // Call gRPC unary method
    // *** GRPC UNARY CALL HERE ***
    const result = await SellerGrpcClient.deleteSeller(id);

    if (!result.success) {
      return res.status(StatusCodes.NOT_FOUND).json({
        success: false,
        message: result.message
      });
    }

    return res.status(StatusCodes.OK).json({
      success: true,
      message: result.message
    });

  } catch (error: any) {
    console.error('Error deleting seller:', error);
    
    const statusCode = mapGrpcErrorToHttp(error);
    
    return res.status(statusCode).json({
      error: 'Failed to delete seller',
      message: error.message || 'Unknown error',
      details: error.details || error.message
    });
  }
});

/**
 * Map gRPC error codes to HTTP status codes
 */
function mapGrpcErrorToHttp(error: any): number {
  // Check for gRPC error code
  const grpcCode = error.code;
  
  switch (grpcCode) {
    case 3: // INVALID_ARGUMENT
      return StatusCodes.BAD_REQUEST;
    case 5: // NOT_FOUND
      return StatusCodes.NOT_FOUND;
    case 6: // ALREADY_EXISTS
      return StatusCodes.CONFLICT;
    case 7: // PERMISSION_DENIED
      return StatusCodes.FORBIDDEN;
    case 16: // UNAUTHENTICATED
      return StatusCodes.UNAUTHORIZED;
    case 14: // UNAVAILABLE
      return StatusCodes.SERVICE_UNAVAILABLE;
    default:
      return StatusCodes.INTERNAL_SERVER_ERROR;
  }
}

export default router;
