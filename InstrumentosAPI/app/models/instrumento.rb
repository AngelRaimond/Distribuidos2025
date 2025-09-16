require 'active_record'

class Instrumento < ActiveRecord::Base
  self.table_name = 'instrumentos'
  
  validates :nombre, presence: true, length: { maximum: 100 }
  validates :marca, presence: true, length: { maximum: 50 }
  validates :precio, presence: true, numericality: { greater_than: 0 }
  validates :modelo, presence: true, length: { maximum: 50 }
  validates :año, presence: true, numericality: { greater_than: 1800, less_than_or_equal_to: Date.current.year }
  validates :linea, presence: true, length: { maximum: 50 }
end