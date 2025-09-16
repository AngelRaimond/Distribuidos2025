require_relative 'config/environment.rb'
require_relative 'app/soap/instrumento_soap_service'

map '/InstrumentoService.svc' do
  run InstrumentoSoapService
end

map '/' do
  run proc { [200, { 'Content-Type' => 'text/plain' }, ['Instrumentos SOAP API - Servicio funcionando']] }
end