# 🎸 Instrumentos API SOAP

<div align="center">

![Ruby](https://img.shields.io/badge/ruby-%23CC342D.svg?style=for-the-badge&logo=ruby&logoColor=white)
![Sinatra](https://img.shields.io/badge/sinatra-%23CC342D.svg?style=for-the-badge&logo=ruby&logoColor=white)
![MySQL](https://img.shields.io/badge/mysql-%2300f.svg?style=for-the-badge&logo=mysql&logoColor=white)
![Podman](https://img.shields.io/badge/podman-892CA0?style=for-the-badge&logo=podman&logoColor=white)
![SOAP](https://img.shields.io/badge/SOAP-FF6B35?style=for-the-badge&logo=soap&logoColor=white)

**Un servicio web SOAP completo para gestión de instrumentos musicales desarrollado en Ruby**

[🚀 Instalación](#-instalación-completa) • [📋 Documentación](#-documentación-de-la-api) • [🧪 Pruebas](#-pruebas-con-insomnia) • [🏗️ Arquitectura](#️-arquitectura-del-proyecto)

</div>

---

##  Descripción del Proyecto

**Instrumentos API SOAP** es un servicio web completo desarrollado en Ruby que permite gestionar un catálogo de instrumentos musicales mediante el protocolo SOAP. El proyecto implementa una arquitectura limpia con separación de responsabilidades y utiliza contenedores para facilitar el despliegue.

###  Características Principales

-  **Servicio SOAP completo** con WSDL automático
-  **CRUD completo** para instrumentos musicales
-  **Arquitectura limpia** con capas bien definidas
-  **Base de datos MySQL** con migraciones automáticas
-  **Contenedores Docker/Podman** para fácil despliegue
-  **Migraciones Automaticas** en la base de datos
-  **Validaciones** de datos
-  **Manejo de errores** 

##  Diferencias con PokemonAPI (.NET)

| Aspecto | Instrumentos API (Ruby) | PokemonAPI (.NET) |
|---------|------------------------|-------------------|
| **Lenguaje** | Ruby con Sinatra | C# con ASP.NET Core |
| **Protocolo** | SOAP | SOAP |
| **Base de Datos** | MySQL | MySQL |
| **Contenedores** | Podman/Docker | Podman/Docker |
| **Validaciones** | Clases validadoras personalizadas | Data Annotations |
| **ORM** | ActiveRecord | Entity Framework Core |

##  Arquitectura del Proyecto

```
InstrumentosAPI/
├── 📁 app/
│   ├── 📁 models/               # Modelos de datos (ActiveRecord)
│   │   └── instrumento.rb
│   ├── 📁 repositories/         # Capa de acceso a datos
│   │   └── instrumento_repository.rb
│   ├── 📁 services/            # Lógica de negocio
│   │   └── service.rb
│   ├── 📁 validators/          # Validaciones de datos
│   │   └── instrumento_validator.rb
│   ├── 📁 mappers/             # Transformación de datos
│   │   └── instrumento_mapper.rb
│   └── 📁 soap/                # Servicios SOAP
│       └── instrumento_soap_service.rb
├── 📁 config/                  # Configuraciones
│   ├── database.yml
│   └── environment.rb
├── 📁 db/                      # Base de datos
│   ├── migrate/                # Migraciones
│   └── migrate.yml             # Config para migraciones
├── 📄 Dockerfile
├── 📄 docker-compose.yml
├── 📄 Gemfile                  # Dependencias Ruby
├── 📄 config.ru               # Configuración Rack
└── 📄 init.sh                 # Script de inicialización
```

##  Documentación de la API

###  Modelo de Datos: Instrumento

```xml
<Instrumento>
    <id>string</id>           <!-- ID único (UUID) -->
    <nombre>string</nombre>   <!-- Nombre del instrumento -->
    <marca>string</marca>     <!-- Marca del fabricante -->
    <precio>double</precio>   <!-- Precio en formato decimal -->
    <modelo>string</modelo>   <!-- Modelo específico -->
    <año>int</año>           <!-- Año de fabricación -->
    <linea>string</linea>     <!-- Línea de producto -->
</Instrumento>
```

### 🔧 Operaciones SOAP Disponibles

| Operación | Descripción | Parámetros |
|-----------|-------------|------------|
| `CreateInstrumento` | Crear nuevo instrumento | nombre, marca, precio, modelo, año, linea |
| `GetInstrumentoById` | Obtener por ID | id |
| `GetInstrumentosByName` | Buscar por nombre | nombre |
| `UpdateInstrumento` | Actualizar instrumento | Busca por id  / actualiza nombre, marca, precio, modelo, año, linea |
| `DeleteInstrumento` | Eliminar instrumento | id |

## 🚀 Instalación Completa

###  Prerrequisitos

Necesitas instalar los siguientes componentes según tu sistema operativo:

####  Windows

1. **Instalar Podman Desktop**:
   - Ve a https://podman-desktop.io/downloads/windows
   - Descarga e instala Podman Desktop
   - Reinicia tu computadora
   - Abre PowerShell como administrador y ejecuta:
   ```powershell
   podman --version
   ```

   si se muestra la versión tu instalación de Podman es correcta

2. **Instalar Git** (si no lo tienes):
   - Ve a https://git-scm.com/download/win
   - Descarga e instala Git for Windows

####  macOS (nota) la instalación en MacOS no ha sido probada

1. **Instalar Podman**:
   ```bash
   # Con Homebrew 
   brew install podman
   
   # O descargar desde https://podman.io/getting-started/installation
   ```

2. **Instalar Git** (si no lo tienes):
   ```bash
   # Con Homebrew
   brew install git
   
   # O usar Xcode Command Line Tools
   xcode-select --install
   ```

####  Linux (Ubuntu/Debian) (nota) la instalación en linux no ha sido probada


1. **Instalar Podman**:
   ```bash
   sudo apt update
   sudo apt install podman
   ```

2. **Instalar Git** (si no lo tienes):
   ```bash
   sudo apt install git
   ```

####  Linux (Fedora/RHEL/CentOS) (nota) la instalación en linux no ha sido probada


1. **Instalar Podman**:
   ```bash
   sudo dnf install podman
   # O para versiones más antiguas: sudo yum install podman
   ```

2. **Instalar Git** (si no lo tienes):
   ```bash
   sudo dnf install git
   ```

###  Clonar el Repositorio

Abre tu terminal/PowerShell ve al directorio donde quieras tener la aplicación con 
```bash
cd Directorio de la carpeta donde quiieres el proyecto
```
y ejecuta:

```bash
git clone https://github.com/AngelRaimond/InstrumetntsApi-SOAP-Ruby.git
```
```bash
cd TuRuta/InstrumetntsApi-SOAP-Ruby
```

###  Ejecutar el Proyecto
En tu PowerShell o tu bash/shell preferido

IMPORTANTE
Asegurate de estar en el directorio del proyecto, si no, ve a el con el comando 
```bash
cd TuRuta/InstrumetntsApi-SOAP-Ruby
```
si no no encontrará podman el Dockerfile


1. **Crear red para los contenedores**:
   ```bash
   podman network create instrumentos-network
   ```

2. **Levantar base de datos MySQL**:
   ```bash
   podman run -d  --name instrumentos-db --network instrumentos-network -e MYSQL_ROOT_PASSWORD=a -e MYSQL_DATABASE=instrumentos_development -p 3309:3306  mysql:8.0
   ```

3. **Construir la imagen de la aplicación**:
   ```bash
   podman build -t instrumentos-api .
   ```

4. **Ejecutar la aplicación**:
   ```bash
   podman run -d --name instrumentos-api  --network instrumentos-network -p 4567:4567 instrumentos-api
   ```

### ✅ Verificar que Todo Funciona

1. **Verificar contenedores activos**:
   ```bash
   podman ps
   ```
   
   Deberías ver algo como:
   ```
   CONTAINER ID  IMAGE                        COMMAND     CREATED         STATUS         PORTS                              NAMES
   abc123...     localhost/instrumentos-api   /bin/bash   2 minutes ago   Up 2 minutes   0.0.0.0:4567->4567/tcp            instrumentos-api
   def456...     docker.io/library/mysql:8.0  mysqld      3 minutes ago   Up 3 minutes   0.0.0.0:3309->3306/tcp, 33060/tcp instrumentos-db
   ```

2. **Verificar logs de la aplicación**:
   ```bash
   podman logs -f instrumentos-api
   ```
   
   Deberías ver:
   ```
   === Iniciando aplicación Instrumentos API ===
   Base de datos conectada exitosamente!
   Iniciando servidor SOAP en puerto 4567...
   ```

3. **Verificar acceso al servicio**:
   - Abre tu navegador
   - Ve a http://localhost:4567/InstrumentoService.svc
   - Deberías ver: "Instrumentos SOAP Service - Use POST for SOAP requests or add ?wsdl for WSDL"

4. **Verificar WSDL**:
   - Ve a http://localhost:4567/InstrumentoService.svc?wsdl
   - Deberías ver un XML con la definición del servicio

##  Pruebas con Insomnia

###  Instalar Insomnia

#### Windows
1. Ve a https://insomnia.rest/download
2. Descarga "Insomnia for Windows"
3. Ejecuta el instalador

#### macOS Nota (la instalacion mediante comando en MacOS no ha sido probada)
```bash
# Con Homebrew
brew install --cask insomnia

# O descargar desde https://insomnia.rest/download
```

#### Linux (la instalacion mediante comando en Linux no ha sido probada)
```bash
# Ubuntu/Debian
sudo snap install insomnia

# O descargar .deb desde https://insomnia.rest/download
sudo dpkg -i insomnia_*.deb
```

### 🔧 Configurar Insomnia para SOAP

1. **Abrir Insomnia** y crear nuevo proyecto:
   - Click en "Create" → "Request Collection"
   - Nombre: "Instrumentos SOAP API"

2. **Verificar conectividad**:
   - Crear nuevo request: de metodo GET al link `http://localhost:4567/InstrumentoService.svc`
   - Debería retornar mensaje de confirmación

3. **Obtener WSDL**:
   - Crear nuevo request de metodo GET al link: `http://localhost:4567/InstrumentoService.svc?wsdl`
   - Debería retornar XML con definición del servicio

###  Ejemplos de Pruebas SOAP
Para todas las pruebas de los metodos:
vas a crear una nueva request a la colección en el simbolo de +
vas a llenar los campos con:
**Request:**
- **Method:** POST
- **URL:** `http://localhost:4567/InstrumentoService.svc`
- **Body le das click a la pestaña y vas a SELECCIONAR Raw XML:**
- En el contenido del body vas a pegar el xml de cada peticion que quieras realizar

  Nota
  Puedes crear una nueva peticion para cada metodo o reutilizar la misma cambiando solo el xml, aunque por practicidad de las pruebas recomendaria crear una peticion para cada metodo

#### 1. Crear Instrumento


Pegar (puedes modificar los campos entre nombre, marca, precio, modelo, año y linea)
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:CreateInstrumento>
      <nombre>Guitarra Eléctrica</nombre>
      <marca>Fender</marca>
      <precio>1500.00</precio>
      <modelo>Stratocaster</modelo>
      <año>2023</año>
      <linea>Professional</linea>
    </tns:CreateInstrumento>
  </soap:Body>
</soap:Envelope>
```

**Response esperado:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:CreateInstrumentoResponse>
      <instrumento>
        <id>f47ac10b-58cc-4372-a567-0e02b2c3d479</id>
        <nombre>Guitarra Eléctrica</nombre>
        <marca>Fender</marca>
        <precio>1500.0</precio>
        <modelo>Stratocaster</modelo>
        <año>2023</año>
        <linea>Professional</linea>
      </instrumento>
    </tns:CreateInstrumentoResponse>
  </soap:Body>
</soap:Envelope>
```

#### 2. Obtener Instrumento por ID
entre los <id> tienes que poner un id con formato GUID 

**Request:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:GetInstrumentoById>
      <id>Id con formato GUID</id>
    </tns:GetInstrumentoById>
  </soap:Body>
</soap:Envelope>
```

#### 3. Buscar por Nombre

**Request:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:GetInstrumentosByName>
      <nombre>Guitarra</nombre>
    </tns:GetInstrumentosByName>
  </soap:Body>
</soap:Envelope>
```

#### 4. Actualizar Instrumento

**Request:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:UpdateInstrumento>
      <id>f47ac10b-58cc-4372-a567-0e02b2c3d479</id>
      <nombre>Guitarra Eléctrica Premium</nombre>
      <marca>Fender</marca>
      <precio>1800.00</precio>
      <modelo>Stratocaster</modelo>
      <año>2024</año>
      <linea>Premium</linea>
    </tns:UpdateInstrumento>
  </soap:Body>
</soap:Envelope>
```

#### 5. Eliminar Instrumento

**Request:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://tempuri.org/">
  <soap:Body>
    <tns:DeleteInstrumento>
      <id>f47ac10b-58cc-4372-a567-0e02b2c3d479</id>
    </tns:DeleteInstrumento>
  </soap:Body>
</soap:Envelope>
```


## 🐛 Resolución de Problemas

### ❌ Error: "Port already in use"
```bash
# Encontrar qué está usando el puerto
podman ps -a
# Detener contenedor conflictivo
podman stop [CONTAINER_ID]
```
Ademas, el puerto 4567 es el puerto que usa Sinatra por default, revisar si no se tiene alguna otra instancia de algun servicio de Sinatra corriendo 

### ❌ Error: "Network not found"
```bash
# Crear la red manualmente
podman network create instrumentos-network
```

### ❌ Error: "Connection refused"
```bash
# Verificar que MySQL esté corriendo
podman logs instrumentos-db

# Reiniciar base de datos si es necesario
podman restart instrumentos-db
```


### ❌ Limpiar todo y empezar de nuevo
```bash
# Detener todos los contenedores
podman stop instrumentos-api instrumentos-db

# Eliminar contenedores
podman rm instrumentos-api instrumentos-db

# Eliminar red
podman network rm instrumentos-network

# Eliminar imágenes 
podman rmi instrumentos-api mysql:8.0
```


<div align="center">
Autor

**Raimond Mendoza Angel** Meg
 GitHub: [@Angel Raimond](https://github.com/AngelRaimond)
 Email: angelraimond11@gmail.com

---




</div>
