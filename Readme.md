### ESTRUCTURA GENERAL:
------------------
Este proyecto combina:
- API REST (Node.js + Express) como gateway principal
- Servicio SOAP (Ruby + Sinatra) para gestión de instrumentos
- Servicio gRPC (Python) para gestión de vendedores
- OAuth2 (Hydra) para autenticación
- Redis para caché
- MySQL para instrumentos, DynamoDB para vendedores


###  Levantar todos los servicios

```powershell
podman compose up --build -d
```

**Servicios activos:**
- **MusicShopApi (REST)**: http://localhost:8088
- **InstrumentosAPI (SOAP)**: http://localhost:4567/soap
- **SellerApi (gRPC)**: localhost:50052
- **DynamoDB**: http://localhost:8000
- **OAuth2 Hydra**: http://localhost:4444



##  Testing con Postman

### Importar Colección

1. Abre Postman
2. Click en **Import**
3. Selecciona `postman_collection.json`
4. La colección incluye 4 secciones:
   - **Setup**: Obtener token OAuth2
   - **SOAP API - Direct**: Llamadas directas al servicio SOAP
   - **SOAP API - via REST**: SOAP invocado desde REST
   - **gRPC API - via REST**: gRPC invocado desde REST

### Flujo de Prueba

#### 1. Autenticación (Requerida)
```
Setup > Get OAuth2 Token
```
- Guarda automáticamente el token en variable `access_token`
- Todas las demás peticiones usan este token

#### 2. Probar SOAP Directamente
```
SOAP API - Direct > List All Instruments (SOAP)
SOAP API - Direct > Create Instrument (SOAP)
SOAP API - Direct > Get Instrument by ID (SOAP)
```
- Usa XML/SOAP directamente
- No requiere token OAuth2
- Puerto 4567

#### 3. Probar SOAP vía REST
```
SOAP API - via REST > List All Instruments (REST)
SOAP API - via REST > Create Instrument (REST)
SOAP API - via REST > Get Instrument by ID (REST)
SOAP API - via REST > Update Instrument (REST)
SOAP API - via REST > Delete Instrument (REST)
```
- Usa JSON en lugar de XML
- Requiere token OAuth2
- MusicShopApi invoca SOAP internamente

#### 4. Probar gRPC vía REST
```
gRPC API - via REST > Create Sellers (Client Streaming)
gRPC API - via REST > Search Sellers by Name (Server Streaming)
gRPC API - via REST > Get Seller by ID
gRPC API - via REST > Update Seller
gRPC API - via REST > Delete Seller
```
- Usa JSON en las peticiones REST
- MusicShopApi invoca gRPC internamente
-  3 patrones de streaming:
  - **Client Streaming**: Crear múltiples sellers en una llamada
  - **Server Streaming**: Búsqueda retorna múltiples resultados
  - **Unary**: Get/Update/Delete individuales

#### 5. Probar gRPC Directamente
Dado que postman collections no soporta gRPC directamente, se deberá dar impórt, configurar y ejecutar las peticiones gRPC directas usando la interfaz gRPC de Postman.

ejemplos de metodos con sus respectivos parametros:

```

- CreateSeller (Client Streaming)
Message:
{
  "name": "John Seller",
  "email": "john@test.com",
  "age": 28,
  "hire_date": "2024-01-15",
  "phone": "+1234567890",
  "address": "123 Main St",
  "sales": [
    {
      "instrument_name": "Guitar",
      "amount": 1299.99,
      "sale_date": "2024-11-01"
    }
  ]
}
- GetSellerByName (Server Streaming)
Message:
{
  "name": "J"
}
- GetSellerById (Unary)
Message:
{
  "id": "ID (esto debe ser remplazado por el ID )"
}
- UpdateSeller (Unary)
Message:
{
  "id": "REMPLAZA EL ID",
  "name": "John Updated",
  "email": "john@test.com",
  "age": 29,
  "hire_date": "2024-01-15",
  "phone": "+1234567890",
  "address": "123 Updated Street",
  "sales": [
    {
      "instrument_name": "Guitar",
      "amount": 1499.99,
      "sale_date": "2024-11-01"
    },
    {
      "instrument_name": "Bass",
      "amount": 899.99,
      "sale_date": "2024-11-15"
    }
  ]
}
- DeleteSeller (Unary)
Message:
{
  "id": "REMPLAZA EL ID"
}
```


---



##  Arquitectura

```
┌─────────────────┐
│   Postman/      │
│   Cliente       │
└────────┬────────┘
         │
         │ HTTP/REST + OAuth2
         │
    ┌────▼─────────────────┐
    │   MusicShopApi       │
    │   (Node.js/Express)  │
    │   Port 8088          │
    └─────┬────────────┬───┘
          │            │
          │ SOAP       │ gRPC
          │            │
    ┌─────▼─────┐   ┌──▼────────────┐
    │Instrumentos│   │   SellerApi   │
    │   API      │   │   (Python)    │
    │  (Ruby)    │   │   Port 50052  │
    │ Port 4567  │   └───────┬───────┘
    └─────┬──────┘           │
          │                  │
    ┌─────▼──────┐    ┌──────▼────────┐
    │   MySQL    │    │   DynamoDB    │
    │  Port 3306 │    │   Port 8000   │
    └────────────┘    └───────────────┘
```

### Flujos de Datos

**1. SOAP Directo:**
```
Cliente → InstrumentosAPI (SOAP) → MySQL
```

**2. SOAP via REST:**
```
Cliente → MusicShopApi (REST) → InstrumentosAPI (SOAP) → MySQL
```

**3. gRPC via REST:**
```
Cliente → MusicShopApi (REST) → SellerApi (gRPC) → DynamoDB
```

---

##  Validaciones 

### InstrumentosAPI (SOAP)
- `name`: Requerido, string
- `category`: Requerido, string
- `price`: Requerido, numérico > 0
- `stock`: Requerido, entero >= 0

### SellerApi (gRPC)
- `name`: Requerido, string
- `email`: Requerido, formato email válido, único
- `age`: Requerido, >= 18
- `hire_date`: Requerido, formato ISO (YYYY-MM-DD)
- `phone`: Requerido, string
- `address`: Requerido, string
- `sales`: Array opcional
  - `instrument_name`: Requerido en cada sale
  - `amount`: Requerido, > 0
  - `sale_date`: Requerido, formato ISO

---

##  Patrones de gRPC Implementados

### 1. Client Streaming (CreateSeller)


### 2. Server Streaming (GetSellerByName)


### 3. Unary (GetById, Update, Delete)


---

##  Notas Finales

- Todos los servicios se levantan con **un solo comando**
- La colección Postman tiene tests automáticos que guardan IDs
- La colección Postman genera datos dinámicos para evitar conflictos entre test repetidos
- Los errores gRPC se mapean a códigos HTTP apropiados
- DynamoDB se inicializa automáticamente con la tabla Sellers
- El token OAuth2 se genera automáticamente al inicio

# notas estructurales 
1.1 hydra-clients.json
----------------------
PROPÓSITO: Define los clientes OAuth2 que pueden autenticarse en Hydra.
1.2 requirements.txt (SellerApi/requirements.txt)
-------------------------------------------------
PROPÓSITO: Lista todas las dependencias Python del servicio SellerApi.
2.1 scripts/hydra-init.sh
--------------------------
PROPÓSITO: Inicializar Hydra con cliente OAuth2 usando API REST.
2.2 scripts/hydra-client.sh
----------------------------
PROPÓSITO: Importar cliente desde hydra-clients.json usando Hydra CLI.

2.3 scripts/hydra-token.sh
---------------------------
PROPÓSITO: Obtener token de acceso OAuth2 para testing.



================================================================================
3. FLUJO DE INICIALIZACIÓN DEL SISTEMA
================================================================================

1. BASES DE DATOS
   - hydra-db        → PostgreSQL para Hydra (puerto 5432)
   - instrumentos-db → MySQL para InstrumentosAPI (puerto 3306)
   - dynamodb-local  → DynamoDB Local para SellerApi (puerto 8000)
   - redis           → Redis para caché (puerto 6379)

2. HYDRA SETUP
   - hydra-migrate   → Crea tablas en PostgreSQL
   - hydra           → Servidor OAuth2 (puertos 4444 público, 4445 admin)
   - hydra-client    → Importa cliente 'musicshop' con hydra-client.sh
   - hydra-token     → Genera token y lo guarda en tmp/access_token

3. SERVICIOS DE BACKEND
   - instrumentosapi → SOAP (Ruby) en puerto 4567
   - sellerapi       → gRPC (Python) en puerto 50052
     * Primero ejecuta scripts/init_db.py para crear tabla Sellers en DynamoDB
     * Luego inicia servidor gRPC con main.py

4. API GATEWAY
   - musicshopapi    → Express (Node.js) en puerto 8088
     * Consume SOAP de instrumentosapi
     * Consume gRPC de sellerapi
     * Valida tokens con Hydra
     * Usa Redis para caché
