import axios from 'axios';
const SOAP_URL = process.env.SOAP_URL || 'http://localhost:4567/soap';
function envelope(opName, payloadXml = '') {
    return `<?xml version="1.0" encoding="UTF-8"?>
  <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
    <soapenv:Body>
      <${opName}>${payloadXml}</${opName}>
    </soapenv:Body>
  </soapenv:Envelope>`;
}
export async function soapList() {
    const xml = envelope('ListInstruments');
    const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' } });
    // very naive xml parse: just search tags (safe because our SOAP is small)
    const items = [];
    const re = /<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/gms;
    let m;
    while ((m = re.exec(data))) {
        items.push({
            id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]),
            precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7])
        });
    }
    return items;
}
export async function soapGet(id) {
    const xml = envelope('GetInstrument', `<id>${id}</id>`);
    const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' } });
    const m = data.match(/<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/ms);
    if (!m)
        return null;
    return { id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]), precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7]) };
}
function xmlEscape(text) {
    if (typeof text === 'number')
        return text.toString();
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&apos;');
}
function xmlUnescape(text) {
    return text
        .replace(/&lt;/g, '<')
        .replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&apos;/g, "'")
        .replace(/&amp;/g, '&');
}
export async function soapCreate(instrument) {
    const p = instrument;
    const xml = envelope('CreateInstrument', `<nombre>${xmlEscape(p.nombre)}</nombre>`
        + `<marca>${xmlEscape(p.marca)}</marca>`
        + `<modelo>${xmlEscape(p.modelo)}</modelo>`
        + `<precio>${xmlEscape(p.precio)}</precio>`
        + `<anio>${xmlEscape(p.anio)}</anio>`
        + `<categoria>${xmlEscape(p.categoria)}</categoria>`);
    console.log('SOAP create request:', xml);
    const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' } });
    console.log('SOAP create response:', data);
    return soapExtractInstrument(data);
}
export async function soapUpdate(id, patch) {
    const f = (k) => {
        if (patch[k] === undefined)
            return '';
        return `<${k}>${xmlEscape(patch[k])}</${k}>`;
    };
    const xml = envelope('UpdateInstrument', `<id>${id}</id>${f('nombre')}${f('marca')}${f('modelo')}${f('precio')}${f('anio')}${f('categoria')}`);
    console.log('SOAP update request:', xml);
    const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' } });
    console.log('SOAP update response:', data);
    const hasNotFound = /<e>Not found<\/error>/.test(data);
    if (hasNotFound)
        return null;
    return soapExtractInstrument(data);
}
export async function soapDelete(id) {
    const xml = envelope('DeleteInstrument', `<id>${id}</id>`);
    console.log('SOAP delete request:', xml);
    const { data } = await axios.post(SOAP_URL, xml, { headers: { 'Content-Type': 'text/xml' } });
    console.log('SOAP delete response:', data);
    return /<success>true<\/success>/.test(data);
}
function soapExtractInstrument(xml) {
    const m = xml.match(/<instrument>\s*<id>(\d+)<\/id>\s*<nombre>(.*?)<\/nombre>\s*<marca>(.*?)<\/marca>\s*<modelo>(.*?)<\/modelo>\s*<precio>([\d.]+)<\/precio>\s*<anio>(\d+)<\/anio>\s*<categoria>(.*?)<\/categoria>\s*<\/instrument>/ms);
    if (!m)
        throw new Error('soap parse error');
    return { id: Number(m[1]), nombre: xmlUnescape(m[2]), marca: xmlUnescape(m[3]), modelo: xmlUnescape(m[4]), precio: Number(m[5]), anio: Number(m[6]), categoria: xmlUnescape(m[7]) };
}
