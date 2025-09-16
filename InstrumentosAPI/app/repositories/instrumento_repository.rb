require_relative '../models/instrumento'
require 'securerandom'

class InstrumentoRepository
  def create(instrumento_data)
    instrumento = Instrumento.new(instrumento_data)
    instrumento.id = SecureRandom.uuid
    instrumento.save!
    instrumento
  end

  def find_by_id(id)
    Instrumento.find_by(id: id)
  end

  def find_by_name(nombre)
    Instrumento.where("nombre LIKE ?", "%#{nombre}%").to_a
  end

  def update(instrumento)
    instrumento.save!
    instrumento
  end

  def delete(instrumento)
    instrumento.destroy!
  end

  def exists_by_name?(nombre)
    Instrumento.exists?(nombre: nombre)
  end
end