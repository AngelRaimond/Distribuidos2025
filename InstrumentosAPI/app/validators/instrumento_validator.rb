class InstrumentoValidator
  def self.validate_create_data(data)
    errors = []
    
    errors << "Nombre es requerido" if data[:nombre].nil? || data[:nombre].strip.empty?
    errors << "Marca es requerida" if data[:marca].nil? || data[:marca].strip.empty?
    errors << "Precio debe ser mayor a 0" if data[:precio].nil? || data[:precio] <= 0
    errors << "Modelo es requerido" if data[:modelo].nil? || data[:modelo].strip.empty?
    errors << "Año es requerido" if data[:año].nil? || data[:año] <= 1800 || data[:año] > Date.current.year
    errors << "Línea es requerida" if data[:linea].nil? || data[:linea].strip.empty?
    
    errors
  end

  def self.validate_update_data(data)
    errors = []
    
    errors << "ID es requerido" if data[:id].nil? || data[:id].strip.empty?
    errors.concat(validate_create_data(data))
    
    errors
  end
end