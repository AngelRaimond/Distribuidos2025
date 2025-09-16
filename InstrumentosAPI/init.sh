#!/bin/bash
set -e

echo "=== Iniciando aplicación Instrumentos API ==="

echo "Verificando entorno..."
echo "Ruby version: $(ruby --version)"
echo "Bundle version: $(bundle --version)"
echo "Directorio actual: $(pwd)"
echo "Contenido del directorio:"
ls -la

echo "Esperando a que la base de datos esté lista..."
MAX_ATTEMPTS=30
ATTEMPT=0

until mysql -h instrumentos-db -u root -pa --connect-timeout=5 --skip-ssl -e "SELECT 1" >/dev/null 2>&1; do
    ATTEMPT=$((ATTEMPT + 1))
    if [ $ATTEMPT -gt $MAX_ATTEMPTS ]; then
        echo "ERROR: No se pudo conectar a la base de datos después de $MAX_ATTEMPTS intentos"
        exit 1
    fi
    echo "Esperando MySQL... (intento $ATTEMPT/$MAX_ATTEMPTS)"
    sleep 2
done

echo "Base de datos conectada exitosamente!"

echo "Verificando archivos de configuración..."
if [ ! -f "config/environment.rb" ]; then
    echo "ERROR: config/environment.rb no encontrado"
    exit 1
fi

if [ ! -f "config.ru" ]; then
    echo "ERROR: config.ru no encontrado"
    exit 1
fi

echo "Verificando conexión a base de datos con Ruby..."
timeout 30 bundle exec ruby -e "
require_relative 'config/environment'
puts 'Conexión a ActiveRecord exitosa'
puts 'Base de datos: ' + ActiveRecord::Base.connection.current_database
" || {
    echo "ERROR: No se pudo conectar a la base de datos con ActiveRecord"
    exit 1
}

echo "Listando bases de datos existentes..."
mysql -h instrumentos-db -u root -pa --skip-ssl -e "SHOW DATABASES;" || echo "Advertencia: No se pudo listar bases de datos"

echo "Creando base de datos si no existe..."
timeout 30 bundle exec rake db:create 2>/dev/null || {
    echo "Advertencia: db:create falló o ya existe"
    echo "Intentando crear manualmente..."
    mysql -h instrumentos-db -u root -pa --skip-ssl -e "CREATE DATABASE IF NOT EXISTS instrumentos_development CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" || echo "Base de datos ya existe"
}

echo "Ejecutando migraciones..."
timeout 30 bundle exec rake db:migrate 2>/dev/null || {
    echo "Advertencia: db:migrate falló"
    echo "Verificando estado de la base de datos..."
    mysql -h instrumentos-db -u root -pa --skip-ssl -D instrumentos_development -e "SHOW TABLES;" || echo "No se pudo verificar tablas"
}

echo "Verificando que todas las tablas existan..."
mysql -h instrumentos-db -u root -pa --skip-ssl -D instrumentos_development -e "DESCRIBE instrumentos;" || {
    echo "Tabla instrumentos no existe, creando manualmente..."
    mysql -h instrumentos-db -u root -pa --skip-ssl -D instrumentos_development -e "
    CREATE TABLE IF NOT EXISTS instrumentos (
        id VARCHAR(36) PRIMARY KEY,
        nombre VARCHAR(255) NOT NULL,
        marca VARCHAR(255) NOT NULL,
        precio DECIMAL(10,2) NOT NULL,
        modelo VARCHAR(255) NOT NULL,
        año INT NOT NULL,
        linea VARCHAR(255) NOT NULL,
        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    );"
}

echo "Estado final de la base de datos:"
mysql -h instrumentos-db -u root -pa --skip-ssl -D instrumentos_development -e "SHOW TABLES;"

echo "Iniciando servidor SOAP en puerto 4567..."
echo "Endpoint disponible en: http://localhost:4567/InstrumentoService.svc"
echo "WSDL disponible en: http://localhost:4567/InstrumentoService.svc?wsdl"

exec bundle exec rackup -o 0.0.0.0 -p 4567