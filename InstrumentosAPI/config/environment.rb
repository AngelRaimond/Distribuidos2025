require 'bundler/setup'
require 'active_record'
require 'yaml'
require 'erb'

# Configurar standalone_migrations primero
require 'standalone_migrations'

# Obtener la ruta base del proyecto
BASE_PATH = File.expand_path('..', __dir__)

# Cargar configuración de base de datos
database_config_path = File.join(BASE_PATH, 'config', 'database.yml')
database_config = YAML.load(ERB.new(File.read(database_config_path)).result)
environment = ENV['RACK_ENV'] || 'development'

# Configurar ActiveRecord
ActiveRecord::Base.establish_connection(database_config[environment])

# Cargar modelos después de la conexión
models_path = File.join(BASE_PATH, 'app', 'models', '*.rb')
Dir[models_path].each { |file| require file }