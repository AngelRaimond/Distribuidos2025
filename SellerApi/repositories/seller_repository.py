import os
import boto3
from boto3.dynamodb.conditions import Key, Attr
from botocore.exceptions import ClientError
from typing import List, Optional
import uuid
from datetime import datetime
from decimal import Decimal
from models.seller import Seller, Sale


class SellerRepository:
    """Repository pattern: encapsula acceso a DynamoDB, separa persistencia de lógica de negocio."""
    
    def __init__(self):
        """Conecta a DynamoDB (local port 8000 o AWS según env vars)."""
        self.dynamodb = boto3.resource(
            'dynamodb',
            endpoint_url=os.getenv('DYNAMODB_ENDPOINT', 'http://localhost:8000'),
            region_name=os.getenv('AWS_REGION', 'us-east-1'),
            aws_access_key_id=os.getenv('AWS_ACCESS_KEY_ID', 'test'),
            aws_secret_access_key=os.getenv('AWS_SECRET_ACCESS_KEY', 'test')
        )
        self.table_name = os.getenv('DYNAMODB_TABLE_NAME', 'Sellers')
        self.table = self.dynamodb.Table(self.table_name)
    
    def create(self, seller: Seller) -> Seller:
        """Crea seller: genera UUID, timestamps, valida email único, convierte floats a Decimal."""
        if self.email_exists(seller.email):
            raise ValueError(f"Seller with email {seller.email} already exists")
        
        seller.id = str(uuid.uuid4())
        seller.created_at = datetime.utcnow().isoformat()
        seller.updated_at = seller.created_at
        
        try:
            item_dict = self._convert_floats_to_decimals(seller.to_dict())
            self.table.put_item(
                Item=item_dict,
                ConditionExpression='attribute_not_exists(id)'
            )
            return seller
        except ClientError as e:
            if e.response['Error']['Code'] == 'ConditionalCheckFailedException':
                raise ValueError(f"Seller with ID {seller.id} already exists")
            raise Exception(f"Error creating seller: {str(e)}")
    
    def get_by_id(self, seller_id: str) -> Optional[Seller]:
        """Obtiene seller por ID (get_item eficiente, O(1) por clave primaria)."""
        try:
            response = self.table.get_item(Key={'id': seller_id})
            item = response.get('Item')
            
            if item:
                return Seller.from_dict(item)
            return None
        except ClientError as e:
            raise Exception(f"Error getting seller: {str(e)}")
    
    def get_by_name(self, name: str) -> List[Seller]:
        """Busca por nombre (SCAN ineficiente, OK para demo. En prod usa GSI)."""
        try:
            response = self.table.scan(
                FilterExpression=Attr('name').contains(name)
            )
            
            sellers = []
            for item in response.get('Items', []):
                sellers.append(Seller.from_dict(item))
            
            return sellers
        except ClientError as e:
            raise Exception(f"Error searching sellers by name: {str(e)}")
    
    def update(self, seller_id: str, updates: dict) -> Seller:
        """Actualización parcial: valida existencia/email, usa UpdateExpression con placeholders."""
        existing = self.get_by_id(seller_id)
        if not existing:
            raise ValueError(f"Seller with ID {seller_id} not found")
        
        if 'email' in updates and updates['email'] != existing.email:
            if self.email_exists(updates['email'], exclude_id=seller_id):
                raise ValueError(f"Seller with email {updates['email']} already exists")
        
        update_expr_parts = []
        expr_attr_names = {}
        expr_attr_values = {}
        
        updates['updated_at'] = datetime.utcnow().isoformat()
        updates = self._convert_floats_to_decimals(updates)
        
        # Placeholders evitan conflictos con palabras reservadas de DynamoDB
        for idx, (key, value) in enumerate(updates.items()):
            placeholder = f"#attr{idx}"
            value_placeholder = f":val{idx}"
            update_expr_parts.append(f"{placeholder} = {value_placeholder}")
            expr_attr_names[placeholder] = key
            expr_attr_values[value_placeholder] = value
        
        update_expression = "SET " + ", ".join(update_expr_parts)
        
        try:
            response = self.table.update_item(
                Key={'id': seller_id},
                UpdateExpression=update_expression,
                ExpressionAttributeNames=expr_attr_names,
                ExpressionAttributeValues=expr_attr_values,
                ReturnValues='ALL_NEW'
            )
            
            return Seller.from_dict(response['Attributes'])
        except ClientError as e:
            raise Exception(f"Error updating seller: {str(e)}")
    
    def delete(self, seller_id: str) -> bool:
        """Elimina seller. Retorna True si existía, False si no."""
        try:
            if not self.get_by_id(seller_id):
                return False
            
            self.table.delete_item(Key={'id': seller_id})
            return True
        except ClientError as e:
            raise Exception(f"Error deleting seller: {str(e)}")
    
    def email_exists(self, email: str, exclude_id: Optional[str] = None) -> bool:
        """Verifica email duplicado (SCAN ineficiente, en prod usa GSI). exclude_id para updates."""
        try:
            response = self.table.scan(
                FilterExpression=Attr('email').eq(email)
            )
            
            items = response.get('Items', [])
            
            if exclude_id:
                items = [item for item in items if item['id'] != exclude_id]
            
            return len(items) > 0
        except ClientError as e:
            raise Exception(f"Error checking email existence: {str(e)}")
    
    def get_all(self) -> List[Seller]:
        """Retorna todos los sellers (SCAN sin filtros, en prod usa paginación)."""
        try:
            response = self.table.scan()
            sellers = []
            
            for item in response.get('Items', []):
                sellers.append(Seller.from_dict(item))
            
            return sellers
        except ClientError as e:
            raise Exception(f"Error getting all sellers: {str(e)}")
    
    @staticmethod
    def _convert_floats_to_decimals(obj):
        """Recursivo: convierte float → Decimal(str(obj)) para DynamoDB.
        DynamoDB rechaza floats, usa Decimal(str()) no Decimal() directo."""
        if isinstance(obj, dict):
            return {k: SellerRepository._convert_floats_to_decimals(v) for k, v in obj.items()}
        elif isinstance(obj, list):
            return [SellerRepository._convert_floats_to_decimals(item) for item in obj]
        elif isinstance(obj, float):
            return Decimal(str(obj))
        else:
            return obj
