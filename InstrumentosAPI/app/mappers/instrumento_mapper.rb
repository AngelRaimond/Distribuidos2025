class InstrumentoMapper
  def self.to_response(instrumento)
    return nil if instrumento.nil?
    
    {
      id: instrumento.id,
      nombre: instrumento.nombre,
      marca: instrumento.marca,
      precio: instrumento.precio.to_f,
      modelo: instrumento.modelo,
      año: instrumento.año,
      linea: instrumento.linea
    }
  end

  def self.to_response_list(instrumentos)
    instrumentos.map { |instrumento| to_response(instrumento) }
  end

  def self.from_create_request(data)
    {
      nombre: data[:nombre],
      marca: data[:marca],
      precio: data[:precio],
      modelo: data[:modelo],
      año: data[:año],
      linea: data[:linea]
    }
  end

  def self.from_update_request(data)
    {
      id: data[:id],
      nombre: data[:nombre],
      marca: data[:marca],
      precio: data[:precio],
      modelo: data[:modelo],
      año: data[:año],
      linea: data[:linea]
    }
  end
end