import grpc
from concurrent import futures
import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'generated'))

import seller_pb2
import seller_pb2_grpc

from models.seller import Seller, Sale
from repositories.seller_repository import SellerRepository


class SellerService(seller_pb2_grpc.SellerServiceServicer):
    """Servicio gRPC para Sellers. Separa lógica de negocio (service) de persistencia (repository)."""
    
    def __init__(self):
        self.repository = SellerRepository()
    
    def CreateSeller(self, request_iterator, context):
        """Client Streaming: recibe múltiples sellers (stream), retorna resumen.
        Acumula errores y crea todos los sellers válidos."""
        sellers_created = 0
        seller_ids = []
        errors = []
        
        try:
            for request in request_iterator:
                try:
                    sales = [
                        Sale(
                            instrument_name=sale.instrument_name,
                            amount=sale.amount,
                            sale_date=sale.sale_date
                        )
                        for sale in request.sales
                    ]
                    
                    seller = Seller(
                        name=request.name,
                        email=request.email,
                        age=request.age,
                        hire_date=request.hire_date,
                        sales=sales,
                        phone=request.phone,
                        address=request.address
                    )
                    
                    validation_errors = seller.validate()
                    if validation_errors:
                        errors.append(f"Seller '{request.name}': {', '.join(validation_errors)}")
                        continue
                    
                    created_seller = self.repository.create(seller)
                    sellers_created += 1
                    seller_ids.append(created_seller.id)
                    
                except ValueError as e:
                    errors.append(f"Seller '{request.name}': {str(e)}")
                    continue
                except Exception as e:
                    errors.append(f"Seller '{request.name}': Unexpected error - {str(e)}")
                    continue
            
            if sellers_created == 0 and errors:
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details('; '.join(errors))
                return seller_pb2.CreateSellerResponse()
            
            message = f"Successfully created {sellers_created} seller(s)"
            if errors:
                message += f". Errors: {'; '.join(errors)}"
            
            return seller_pb2.CreateSellerResponse(
                sellers_created=sellers_created,
                seller_ids=seller_ids,
                message=message
            )
            
        except Exception as e:
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Internal error: {str(e)}")
            return seller_pb2.CreateSellerResponse()
    
    def GetSellerByName(self, request, context):
        """Server Streaming: recibe nombre, envía stream de sellers (yield)."""
        try:
            if not request.name or not request.name.strip():
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details("Name parameter is required")
                return
            
            sellers = self.repository.get_by_name(request.name)
            
            if not sellers:
                context.set_code(grpc.StatusCode.NOT_FOUND)
                context.set_details(f"No sellers found with name containing '{request.name}'")
                return
            
            for seller in sellers:
                yield self._seller_to_response(seller)
                
        except Exception as e:
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Internal error: {str(e)}")
            return
    
    def GetSellerById(self, request, context):
        """Unary: request y response simples (como REST GET /sellers/:id)."""
        try:
            if not request.id or not request.id.strip():
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details("Seller ID is required")
                return seller_pb2.SellerResponse()
            
            seller = self.repository.get_by_id(request.id)
            
            if not seller:
                context.set_code(grpc.StatusCode.NOT_FOUND)
                context.set_details(f"Seller with ID {request.id} not found")
                return seller_pb2.SellerResponse()
            
            return self._seller_to_response(seller)
            
        except Exception as e:
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Internal error: {str(e)}")
            return seller_pb2.SellerResponse()
    
    def UpdateSeller(self, request, context):
        """Unary: actualización parcial (usa HasField para detectar campos enviados)."""
        try:
            if not request.id or not request.id.strip():
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details("Seller ID is required")
                return seller_pb2.SellerResponse()
            
            updates = {}
            
            if request.HasField('name'):
                if not request.name.strip():
                    context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                    context.set_details("Name cannot be empty")
                    return seller_pb2.SellerResponse()
                updates['name'] = request.name
            
            if request.HasField('email'):
                updates['email'] = request.email
            
            if request.HasField('age'):
                if request.age < 18:
                    context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                    context.set_details("Age must be at least 18")
                    return seller_pb2.SellerResponse()
                updates['age'] = request.age
            
            if request.HasField('hire_date'):
                updates['hire_date'] = request.hire_date
            
            if request.HasField('phone'):
                updates['phone'] = request.phone
            
            if request.HasField('address'):
                updates['address'] = request.address
            
            if request.sales:
                sales = [
                    {
                        'instrument_name': sale.instrument_name,
                        'amount': sale.amount,
                        'sale_date': sale.sale_date
                    }
                    for sale in request.sales
                ]
                updates['sales'] = sales
                updates['total_sales'] = sum(sale.amount for sale in request.sales)
            
            if not updates:
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details("At least one field must be provided for update")
                return seller_pb2.SellerResponse()
            
            updated_seller = self.repository.update(request.id, updates)
            return self._seller_to_response(updated_seller)
            
        except ValueError as e:
            error_msg = str(e).lower()
            if 'not found' in error_msg:
                context.set_code(grpc.StatusCode.NOT_FOUND)
            elif 'already exists' in error_msg:
                context.set_code(grpc.StatusCode.ALREADY_EXISTS)
            else:
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
            context.set_details(str(e))
            return seller_pb2.SellerResponse()
        except Exception as e:
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Internal error: {str(e)}")
            return seller_pb2.SellerResponse()
    
    def DeleteSeller(self, request, context):
        """Unary: elimina seller. Retorna success en mensaje y código gRPC."""
        try:
            if not request.id or not request.id.strip():
                context.set_code(grpc.StatusCode.INVALID_ARGUMENT)
                context.set_details("Seller ID is required")
                return seller_pb2.DeleteSellerResponse(success=False, message="Seller ID is required")
            
            deleted = self.repository.delete(request.id)
            
            if not deleted:
                context.set_code(grpc.StatusCode.NOT_FOUND)
                return seller_pb2.DeleteSellerResponse(
                    success=False,
                    message=f"Seller with ID {request.id} not found"
                )
            
            return seller_pb2.DeleteSellerResponse(
                success=True,
                message=f"Seller with ID {request.id} successfully deleted"
            )
            
        except Exception as e:
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Internal error: {str(e)}")
            return seller_pb2.DeleteSellerResponse(success=False, message=str(e))
    
    def _seller_to_response(self, seller: Seller) -> seller_pb2.SellerResponse:
        """Helper: convierte modelo Seller a mensaje protobuf SellerResponse."""
        sales = [
            seller_pb2.Sale(
                instrument_name=sale.instrument_name,
                amount=sale.amount,
                sale_date=sale.sale_date
            )
            for sale in seller.sales
        ]
        
        return seller_pb2.SellerResponse(
            id=seller.id,
            name=seller.name,
            email=seller.email,
            age=seller.age,
            hire_date=seller.hire_date,
            sales=sales,
            phone=seller.phone,
            address=seller.address,
            total_sales=seller.total_sales or 0.0,
            created_at=seller.created_at or '',
            updated_at=seller.updated_at or ''
        )
