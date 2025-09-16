require_relative '../repositories/instrumento_repository'
require_relative '../validators/instrumento_validator'
require_relative '../mappers/instrumento_mapper'

class InstrumentoService
  def initialize
    @repository = InstrumentoRepository.new
  end

  def create_instrumento(data)
    # Validar datos
    errors = InstrumentoValidator.validate_create_data(data)
    raise StandardError.new("Errores de validación: #{errors.join(', ')}") unless errors.empty?

    # Verificar si ya existe
    if @repository.exists_by_name?(data[:nombre])
      raise StandardError.new("El instrumento ya existe")
    end

    # Crear instrumento
    instrumento_data = InstrumentoMapper.from_create_request(data)
    instrumento = @repository.create(instrumento_data)
    
    InstrumentoMapper.to_response(instrumento)
  end

  def get_instrumento_by_id(id)
    instrumento = @repository.find_by_id(id)
    raise StandardError.new("Instrumento no encontrado") if instrumento.nil?
    
    InstrumentoMapper.to_response(instrumento)
  end

  def get_instrumentos_by_name(nombre)
    instrumentos = @repository.find_by_name(nombre)
    InstrumentoMapper.to_response_list(instrumentos)
  end

  def update_instrumento(data)
    # Validar datos
    errors = InstrumentoValidator.validate_update_data(data)
    raise StandardError.new("Errores de validación: #{errors.join(', ')}") unless errors.empty?

    # Buscar instrumento existente
    instrumento = @repository.find_by_id(data[:id])
    raise StandardError.new("Instrumento no encontrado") if instrumento.nil?

    # Verificar nombre duplicado (si cambió)
    if data[:nombre] != instrumento.nombre && @repository.exists_by_name?(data[:nombre])
      raise StandardError.new("Ya existe un instrumento con ese nombre")
    end

    # Actualizar instrumento
    instrumento.nombre = data[:nombre]
    instrumento.marca = data[:marca]
    instrumento.precio = data[:precio]
    instrumento.modelo = data[:modelo]
    instrumento.año = data[:año]
    instrumento.linea = data[:linea]

    updated_instrumento = @repository.update(instrumento)
    InstrumentoMapper.to_response(updated_instrumento)
  end

  def delete_instrumento(id)
    instrumento = @repository.find_by_id(id)
    raise StandardError.new("Instrumento no encontrado") if instrumento.nil?

    @repository.delete(instrumento)
    { success: true }
  end
end