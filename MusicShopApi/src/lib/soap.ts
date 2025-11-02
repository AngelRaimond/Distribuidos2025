import axios from 'axios';
import { Instrument } from '../types.js';

const SOAP_URL = process.env.SOAP_URL || 'http://localhost:4567/soap';

function envelope(opName: string, payloadXml: string = ''): string {
  return `<?xml version="1.0" encoding="UTF-8"?>
  <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
    <soapenv:Body>
      <${opName}>${payloadXml}</${opName}>
    </soapenv:Body>
  </soapenv:Envelope>`;
}

export async function soapList(): Promise<Instrument[]> {
  const xml = envelope('ListInstruments');
  const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' }});
  // very naive xml parse: just search tags (safe because our SOAP is small)
  const items: Instrument[] = [];
  const re = /<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/gms;
  let m: RegExpExecArray | null;
  while ((m = re.exec(data))) {
    items.push({
      id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]),
      precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7])
    });
  }
  return items;
}

export async function soapGet(id: number): Promise<Instrument | null> {
  const xml = envelope('GetInstrument', `<id>${id}</id>`);
  const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' }});
  const m = data.match(/<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/ms);
  if (!m) return null;
  return { id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]), precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7]) };
}

function xmlEscape(text: string | number): string {
  if (typeof text === 'number') return text.toString();
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

function xmlUnescape(text: string): string {
  return text
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&apos;/g, "'")
    .replace(/&amp;/g, '&');
}

export async function soapCreate(instrument: Instrument): Promise<Instrument> {
  const p = instrument;
  const xml = envelope('CreateInstrument',
    `<nombre>${xmlEscape(p.nombre)}</nombre>`
    + `<marca>${xmlEscape(p.marca)}</marca>`
    + `<modelo>${xmlEscape(p.modelo)}</modelo>`
    + `<precio>${xmlEscape(p.precio)}</precio>`
    + `<anio>${xmlEscape(p.anio)}</anio>`
    + `<categoria>${xmlEscape(p.categoria)}</categoria>`
  );
  console.log('SOAP create request:', xml);
  const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' }});
  console.log('SOAP create response:', data);
  return soapExtractInstrument(data);
}

export async function soapUpdate(id: number, patch: Partial<Instrument>): Promise<Instrument | null> {
  const f = (k: keyof Instrument) => {
    if (patch[k] === undefined) return '';
    return `<${k}>${xmlEscape((patch as any)[k])}</${k}>`;
  };
  const xml = envelope('UpdateInstrument', `<id>${id}</id>${f('nombre')}${f('marca')}${f('modelo')}${f('precio')}${f('anio')}${f('categoria')}`);
  console.log('SOAP update request:', xml);
  const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' }});
  console.log('SOAP update response:', data);
  const hasNotFound = /<e>Not found<\/error>/.test(data);
  if (hasNotFound) return null;
  return soapExtractInstrument(data);
}

export async function soapDelete(id: number): Promise<boolean> {
  const xml = envelope('DeleteInstrument', `<id>${id}</id>`);
  console.log('SOAP delete request:', xml);
  const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' }});
  console.log('SOAP delete response:', data);
  return /<success>true<\/success>/.test(data);
}

function soapExtractInstrument(xml: string): Instrument {
  const m = xml.match(/<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/ms);
  if (!m) throw new Error('soap parse error');
  return { id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]), precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7]) };
}
