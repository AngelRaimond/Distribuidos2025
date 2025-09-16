class CreateInstrumentos < ActiveRecord::Migration[7.0]
  def up
    create_table :instrumentos, id: false do |t|
      t.string :id, limit: 36, primary_key: true
      t.string :nombre, null: false, limit: 100
      t.string :marca, null: false, limit: 50
      t.decimal :precio, precision: 10, scale: 2, null: false
      t.string :modelo, null: false, limit: 50
      t.integer :año, null: false
      t.string :linea, null: false, limit: 50
      t.timestamps
    end
    
    add_index :instrumentos, :nombre
    add_index :instrumentos, :marca
  end

  def down
    drop_table :instrumentos
  end
end