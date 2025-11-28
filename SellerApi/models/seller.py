from dataclasses import dataclass
from typing import List, Optional
from datetime import datetime
import re
from email_validator import validate_email, EmailNotValidError


@dataclass
class Sale:
    """Venta de un instrumento. @dataclass genera __init__, __repr__ automáticamente."""
    instrument_name: str
    amount: float
    sale_date: str  # ISO format YYYY-MM-DD
    
    def validate(self):
        """Retorna lista de errores de validación (vacía si es válido)."""
        errors = []
        
        if not self.instrument_name or not self.instrument_name.strip():
            errors.append("Instrument name is required")
        
        if self.amount <= 0:
            errors.append("Sale amount must be greater than 0")
        
        if not self._is_valid_date(self.sale_date):
            errors.append("Sale date must be in ISO format (YYYY-MM-DD)")
        
        return errors
    
    @staticmethod
    def _is_valid_date(date_str: str) -> bool:
        try:
            datetime.fromisoformat(date_str)
            return True
        except (ValueError, TypeError):
            return False


@dataclass
class Seller:
    """Vendedor con campos opcionales al final (id, timestamps se generan automáticamente)."""
    name: str
    email: str
    age: int
    hire_date: str  # ISO format YYYY-MM-DD
    sales: List[Sale]
    phone: str
    address: str
    id: Optional[str] = None
    total_sales: Optional[float] = None
    created_at: Optional[str] = None
    updated_at: Optional[str] = None
    
    def __post_init__(self):
        """Calcula total_sales automáticamente si no fue proporcionado."""
        if self.total_sales is None:
            self.total_sales = sum(sale.amount for sale in self.sales)
    
    def validate(self, check_required: bool = True) -> List[str]:
        """Valida seller. check_required=True para CREATE, False para UPDATE (validación parcial)."""
        errors = []
        
        if check_required:
            if not self.name or not self.name.strip():
                errors.append("Name is required and cannot be empty")
            if not self.email:
                errors.append("Email is required")
            if self.age is None:
                errors.append("Age is required")
            if not self.hire_date:
                errors.append("Hire date is required")
            if not self.phone:
                errors.append("Phone is required")
            if not self.address:
                errors.append("Address is required")
        
        # check_deliverability=False evita DNS lookups en contenedores sin internet
        if self.email:
            try:
                validate_email(self.email, check_deliverability=False)
            except EmailNotValidError:
                errors.append("Email format is invalid")
        
        if self.age is not None:
            if self.age < 18:
                errors.append("Age must be at least 18 years old")
            if self.age > 100:
                errors.append("Age must be less than or equal to 100")
        
        if self.hire_date and not self._is_valid_date(self.hire_date):
            errors.append("Hire date must be in ISO format (YYYY-MM-DD)")
        
        if self.phone and not re.match(r'^[\d\s\-\+\(\)]+$', self.phone):
            errors.append("Phone number contains invalid characters")
        
        if self.sales:
            for idx, sale in enumerate(self.sales):
                sale_errors = sale.validate()
                for error in sale_errors:
                    errors.append(f"Sale {idx + 1}: {error}")
        
        return errors
    
    @staticmethod
    def _is_valid_date(date_str: str) -> bool:
        try:
            datetime.fromisoformat(date_str)
            return True
        except (ValueError, TypeError):
            return False
    
    def to_dict(self) -> dict:
        """Convierte Seller a dict para DynamoDB. Genera timestamps si no existen."""
        return {
            'id': self.id,
            'name': self.name,
            'email': self.email,
            'age': self.age,
            'hire_date': self.hire_date,
            'sales': [
                {
                    'instrument_name': sale.instrument_name,
                    'amount': sale.amount,
                    'sale_date': sale.sale_date
                }
                for sale in self.sales
            ],
            'phone': self.phone,
            'address': self.address,
            'total_sales': self.total_sales,
            'created_at': self.created_at or datetime.utcnow().isoformat(),
            'updated_at': self.updated_at or datetime.utcnow().isoformat()
        }
    
    @classmethod
    def from_dict(cls, data: dict) -> 'Seller':
        """Constructor alternativo desde dict. Convierte Decimal a float."""
        sales = [
            Sale(
                instrument_name=sale['instrument_name'],
                amount=float(sale['amount']),
                sale_date=sale['sale_date']
            )
            for sale in data.get('sales', [])
        ]
        
        return cls(
            id=data.get('id'),
            name=data['name'],
            email=data['email'],
            age=int(data['age']),
            hire_date=data['hire_date'],
            sales=sales,
            phone=data['phone'],
            address=data['address'],
            total_sales=float(data.get('total_sales', 0)),
            created_at=data.get('created_at'),
            updated_at=data.get('updated_at')
        )
