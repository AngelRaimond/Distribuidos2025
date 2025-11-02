require 'sinatra/base'
require 'builder'
require 'nokogiri'
require_relative '../services/service'

class InstrumentoSoapService < Sinatra::Base
  def initialize
    super
    @service = InstrumentoService.new
  end

  helpers do
    def parse_xml_from_request
      raw = request.body.read
      halt 400, fault('Client','Empty body') if raw.nil? || raw.strip.empty?
      Nokogiri::XML(raw) { |cfg| cfg.strict.noblanks }
    rescue Nokogiri::XML::SyntaxError => e
      halt 400, fault('Client',"Invalid XML: #{e.message}")
    end
    def fault(code, message)
      builder = Builder::XmlMarkup.new
      builder.instruct!
      builder.soapenv(:Envelope, 'xmlns:soapenv' => 'http://schemas.xmlsoap.org/soap/envelope/') do
        builder.soapenv(:Body) do
          builder.soapenv(:Fault) do
            builder.faultcode code
            builder.faultstring message
          end
        end
      end
    end

    def envelope
      b = Builder::XmlMarkup.new
      b.instruct!
      b.soapenv(:Envelope, 'xmlns:soapenv' => 'http://schemas.xmlsoap.org/soap/envelope/') do
        yield b
      end
    end
    def op_name(doc)
      body = doc.at_xpath('//*[local-name()="Body"]')
      body&.element_children&.first&.name
    end

    def op_params(doc)
      params = {}
      body = doc.at_xpath('//*[local-name()="Body"]')
      op = body&.element_children&.first
      op&.element_children&.each { |el| params[el.name.to_sym] = el.content }
      params
    end
  end

  get '/soap' do
    content_type 'text/xml'
    builder = Builder::XmlMarkup.new
    builder.instruct!
    builder.definitions('name' => 'InstrumentoService',
                        'targetNamespace' => 'http://example.com/instrumentos',
                        'xmlns:tns' => 'http://example.com/instrumentos',
                        'xmlns:soap' => 'http://schemas.xmlsoap.org/wsdl/soap/',
                        'xmlns:wsdl' => 'http://schemas.xmlsoap.org/wsdl/') do
      builder.service('name' => 'InstrumentoService') do
        builder.port('name' => 'InstrumentoServicePort', 'binding' => 'tns:InstrumentoServiceBinding') do
          builder.tag!('soap:address', 'location' => "#{request.base_url}/soap")
        end
      end
    end
  end
  post '/soap' do
    content_type 'text/xml'
    doc = parse_xml_from_request
    name = op_name(doc)
    p = op_params(doc)

    case name
    when 'ListInstruments'
      list = @service.list_instrumentos
      envelope do |x|
        x.soapenv :Body do
          x.ListInstrumentsResponse do
            x.instruments do
              list.each do |i|
                x.instrument do
                  x.id i[:id]
                  x.nombre i[:nombre]
                  x.marca i[:marca]
                  x.modelo i[:modelo]
                  x.precio i[:precio]
                  x.anio i[:anio]
                  x.categoria i[:categoria]
                end
              end
            end
          end
        end
      end
    when 'GetInstrument'
      it = @service.get_instrument_by_id(p[:id])
      envelope do |x|
        x.soapenv :Body do
          x.GetInstrumentResponse do
            if it
              x.instrument do
                x.id it[:id]
                x.nombre it[:nombre]
                x.marca it[:marca]
                x.modelo it[:modelo]
                x.precio it[:precio]
                x.anio it[:anio]
                x.categoria it[:categoria]
              end
            else
              x.error 'Not found'
            end
          end
        end
      end
    when 'CreateInstrument'
      begin
        it = @service.create_instrumento(
          nombre: p[:nombre], marca: p[:marca], modelo: p[:modelo],
          precio: p[:precio], anio: p[:anio], categoria: p[:categoria]
        )
        envelope do |x|
          x.soapenv :Body do
            x.CreateInstrumentResponse do
              x.instrument do
                x.id it[:id]; x.nombre it[:nombre]; x.marca it[:marca]
                x.modelo it[:modelo]; x.precio it[:precio]; x.anio it[:anio]; x.categoria it[:categoria]
              end
            end
          end
        end
      rescue => e
        halt 400, fault('Client', e.message)
      end
    when 'UpdateInstrument'
      begin
        it = @service.update_instrumento(
          id: p[:id], nombre: p[:nombre], marca: p[:marca], modelo: p[:modelo],
          precio: p[:precio], anio: p[:anio], categoria: p[:categoria]
        )
        envelope do |x|
          x.soapenv :Body do
            x.UpdateInstrumentResponse do
              if it
                x.instrument do
                  x.id it[:id]; x.nombre it[:nombre]; x.marca it[:marca]
                  x.modelo it[:modelo]; x.precio it[:precio]; x.anio it[:anio]; x.categoria it[:categoria]
                end
              else
                x.error 'Not found'
              end
            end
          end
        end
      rescue => e
        halt 400, fault('Client', e.message)
      end
    when 'DeleteInstrument'
      ok = @service.delete_instrumento(p[:id])
      envelope do |x|
        x.soapenv :Body do
          x.DeleteInstrumentResponse do
            x.success ok ? 'true' : 'false'
          end
        end
      end
    else
      halt 500, fault('Server', "Unknown operation: #{name}")
    end
  end
  error do
    content_type 'text/xml'
    fault('Server', env['sinatra.error']&.message || 'Server error')
  end
end