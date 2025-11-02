class Instrument
  attr_accessor :id, :nombre, :marca, :modelo, :precio, :anio, :categoria

  def initialize(h = {})
    @id = h[:id]
    @nombre = h[:nombre]
    @marca = h[:marca]
    @modelo = h[:modelo]
    @precio = h[:precio]
    @anio = h[:anio]
    @categoria = h[:categoria]
  end
end
