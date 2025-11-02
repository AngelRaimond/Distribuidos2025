
begin
  require 'bundler/setup'
rescue LoadError
end

require 'rack'
begin
  require 'rack/handler/webrick'
rescue LoadError

  require 'webrick'
end


Dir.glob("./app/**/*.rb").each do |f|
  require_relative f
end


klass = Object.const_get(ENV.fetch('SOAP_APP_CLASS', 'InstrumentoSoapService'))
app = klass.method(:new).arity == 0 ? klass.new : klass

if defined?(Rack::Handler) && defined?(Rack::Handler::WEBrick)
  Rack::Handler::WEBrick.run(app, Host: '0.0.0.0', Port: 4567)
elsif defined?(Rack::Handler) && Rack::Handler.respond_to?(:get)
  handler = Rack::Handler.get('webrick')
  handler.run(app, Host: '0.0.0.0', Port: 4567)
else

  require 'webrick'
  server = WEBrick::HTTPServer.new(Port: 4567, BindAddress: '0.0.0.0')
  server.mount_proc '/' do |req, res|
    env = req.meta_vars
    env['rack.input'] = StringIO.new(req.body || '')
    status, headers, body = app.call(env)
    res.status = status
    headers.each { |k, v| res[k] = v }
    body.each { |chunk| res.body << chunk.to_s }
    body.close if body.respond_to?(:close)
  end
  trap('INT') { server.shutdown }
  server.start
end
