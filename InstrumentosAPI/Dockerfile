# Usar imagen base oficial de Ruby
FROM ruby:3.2-slim

# Instalar dependencias del sistema completas
RUN apt-get update && apt-get install -y \
    build-essential \
    default-mysql-client \
    default-libmysqlclient-dev \
    git \
    bash \
    dos2unix \
    libyaml-dev \
    libffi-dev \
    libssl-dev \
    zlib1g-dev \
    libxml2-dev \
    libxslt1-dev \
    pkg-config \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Crear directorio de trabajo
WORKDIR /app

# Copiar solo el Gemfile primero
COPY Gemfile ./

# Instalar dependencias sin usar el lockfile problemático
RUN bundle config set --local without 'development test' && \
    bundle install

# Copiar el resto de archivos después de instalar dependencias
COPY . .

# Convertir terminaciones de línea y dar permisos
RUN dos2unix ./init.sh && \
    chmod +x ./init.sh

# Verificar que el archivo existe y es ejecutable (sin comando file)
RUN ls -la ./init.sh

# Exponer puerto
EXPOSE 4567

# Usar bash explícitamente para ejecutar el script
CMD ["bash", "./init.sh"]