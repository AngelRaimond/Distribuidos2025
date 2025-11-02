import axios from 'axios';
import http from 'node:http';
const rawIntrospectUrl = process.env.HYDRA_INTROSPECTION_URL || 'http://localhost:4445/oauth2/introspect';
let introspectUrl;
try {
    const u = new URL(rawIntrospectUrl);
    const host = u.hostname;
    if ((host === 'localhost' || host === '127.0.0.1') && process.env.NODE_ENV === 'production') {
        u.hostname = process.env.HYDRA_INTERNAL_HOST || 'hydra';
    }
    introspectUrl = u.toString();
}
catch {
    introspectUrl = rawIntrospectUrl;
}
const clientId = process.env.HYDRA_CLIENT_ID || 'musicshop';
const clientSecret = process.env.HYDRA_CLIENT_SECRET || 'secret';
export async function authMiddleware(req, res, next) {
    const auth = req.headers.authorization || '';
    const m = auth.match(/^Bearer\s+(.+)$/i);
    if (!m)
        return res.status(401).json({ error: 'missing bearer token' });
    try {
        console.log('Token:', m[1]);
        console.log('Hydra introspect URL:', introspectUrl, 'raw:', rawIntrospectUrl, 'env NODE_ENV:', process.env.NODE_ENV);
        const token = m[1];
        // First try HTTP Basic auth (most reliable with Ory Hydra)
        // @ts-ignore Buffer is available in Node runtime; ensure @types/node in build
        const basic = Buffer.from(`${clientId}:${clientSecret}`).toString('base64');
        const bodyBasic = new URLSearchParams();
        bodyBasic.set('token', token);
        console.log('Introspecting (basic) with token only');
        let data;
        try {
            ({ data } = await axios.post(introspectUrl, bodyBasic, {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'Authorization': `Basic ${basic}`
                },
                timeout: 5000,
                proxy: false,
                httpAgent: new http.Agent({ keepAlive: true, family: 4 }),
                transitional: { clarifyTimeoutError: true }
            }));
        }
        catch (e) {
            const anyErr = e;
            console.warn('Basic auth introspection failed, falling back to client_secret_post', { msg: anyErr?.message, code: anyErr?.code, json: anyErr?.toJSON?.() });
            const params = new URLSearchParams();
            params.set('token', token);
            params.set('client_id', clientId);
            params.set('client_secret', clientSecret);
            console.log('Introspecting (post body) with:', params.toString());
            ({ data } = await axios.post(introspectUrl, params, {
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                timeout: 5000,
                proxy: false,
                httpAgent: new http.Agent({ keepAlive: true, family: 4 }),
                transitional: { clarifyTimeoutError: true }
            }));
        }
        console.log('Introspection response:', data);
        if (!data.active)
            return res.status(401).json({ error: 'inactive token' });
        req.subject = data.sub || 'client';
        const scopeStr = (data.scope || data.scp || '').toString();
        req.scopes = scopeStr.split(' ').map((s) => s.trim()).filter(Boolean);
        next();
    }
    catch (e) {
        const anyErr = e;
        const status = anyErr?.response?.status;
        const body = anyErr?.response?.data;
        const msg = anyErr?.message || 'unknown';
        const code = anyErr?.code;
        console.error('Hydra introspection failed:', { status, msg, code, body, json: anyErr?.toJSON?.() });
        return res.status(401).json({ error: 'introspection_failed' });
    }
}
export function requireScope(required) {
    return (req, res, next) => {
        const scopes = req.scopes || [];
        if (!Array.isArray(scopes) || scopes.length === 0) {
            return res.status(403).json({ error: 'forbidden', reason: 'missing scopes' });
        }
        if (!scopes.includes(required)) {
            return res.status(403).json({ error: 'forbidden', required, have: scopes });
        }
        next();
    };
}
