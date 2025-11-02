require 'sequel'

class InstrumentoService
  def initialize
    host = ENV.fetch('DB_HOST', 'localhost')
    port = ENV.fetch('DB_PORT', '3306')
    db   = ENV.fetch('DB_NAME', 'instrumentos')
    usr  = ENV.fetch('DB_USER', 'instrumentos')
    pwd  = ENV.fetch('DB_PASSWORD', 'instrumentos')

    @db = Sequel.connect("mysql2://#{usr}:#{pwd}@#{host}:#{port}/#{db}?encoding=utf8mb4")
    ensure_schema
  end

  def ensure_schema
    @db.create_table? :instruments do
      primary_key :id
      String  :nombre, null: false
      String  :marca, null: false
      String  :modelo, null: false
      Float   :precio, null: false
      Integer :anio, null: false
      String  :categoria, null: false
      index :nombre
      index :marca
      index :categoria
    end
    @tbl = @db[:instruments]
  end

  def list_instrumentos
    @tbl.all
  end

  def get_instrument_by_id(id)
    return nil unless id
    @tbl.where(id: id.to_i).first
  end

  def create_instrumento(h)
    # validaciones se paso a este nivel
    raise 'invalid nombre' if !h[:nombre] || h[:nombre].strip.empty?
    raise 'invalid marca' if !h[:marca] || h[:marca].strip.empty?
    raise 'invalid modelo' if !h[:modelo] || h[:modelo].strip.empty?
    raise 'invalid categoria' if !h[:categoria] || h[:categoria].strip.empty?
    raise 'invalid anio' unless h[:anio].to_i.between?(1900, 2100)
    raise 'invalid precio' unless h[:precio].to_f > 0

    id = @tbl.insert(
      nombre: h[:nombre],
      marca: h[:marca],
      modelo: h[:modelo],
      precio: h[:precio].to_f,
      anio: h[:anio].to_i,
      categoria: h[:categoria]
    )
    get_instrument_by_id(id)
  end

  def update_instrumento(h)
    id = h[:id]
    return nil unless id
    cur = get_instrument_by_id(id)
    return nil unless cur

    upd = {}
    [:nombre,:marca,:modelo,:categoria].each do |k|
      v = h[k]
      upd[k] = v if v && !v.to_s.strip.empty?
    end
    if h[:anio]
      raise 'invalid anio' unless h[:anio].to_i.between?(1900,2100)
      upd[:anio] = h[:anio].to_i
    end
    if h[:precio]
      raise 'invalid precio' unless h[:precio].to_f > 0
      upd[:precio] = h[:precio].to_f
    end

    @tbl.where(id: id.to_i).update(upd) unless upd.empty?
    get_instrument_by_id(id)
  end

  def delete_instrumento(id)
    return false unless id
    @tbl.where(id: id.to_i).delete > 0
  end
end
