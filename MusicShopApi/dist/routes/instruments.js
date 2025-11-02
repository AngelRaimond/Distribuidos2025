import { Router } from 'express';
import { instrumentSchema } from '../lib/validation.js';
import { cacheGet, cacheSet, cacheDel, cacheVersion, cacheBump } from '../lib/cache.js';
import { soapList, soapGet, soapCreate, soapUpdate, soapDelete } from '../lib/soap.js';
import { requireScope } from '../lib/hydra.js';
const router = Router();
function hateoasInstrument(base, it) {
    return {
        ...it,
        _links: {
            self: { href: `${base}/instruments/${it.id}` },
            update: { href: `${base}/instruments/${it.id}`, method: 'PUT' },
            delete: { href: `${base}/instruments/${it.id}`, method: 'DELETE' },
            list: { href: `${base}/instruments`, method: 'GET' }
        }
    };
}
router.get('/instruments', requireScope('read'), async (req, res) => {
    try {
        console.log('GET /instruments called');
        // Pagination and sorting
        const page = Math.max(1, Number(req.query.page || 1));
        const pageSize = Math.max(1, Math.min(100, Number(req.query.pageSize || 10)));
        const sort = req.query.sort || '';
        const v = await cacheVersion('list');
        const cacheKey = `list:v1:${v}:page=${page}:size=${pageSize}:sort=${sort}`;
        const cached = await cacheGet(cacheKey);
        const base = `${req.protocol}://${req.get('host')}`;
        console.log('Base URL:', base);
        if (cached) {
            console.log('Returning cached response');
            res.set('X-Cache-Status', 'HIT');
            return res.json(cached);
        }
        let list = await soapList();
        // sort
        if (sort && ['nombre', 'marca', 'modelo', 'precio', 'anio', 'categoria'].includes(sort)) {
            list = [...list].sort((a, b) => a[sort] > b[sort] ? 1 : (a[sort] < b[sort] ? -1 : 0));
        }
        const total = list.length;
        const start = (page - 1) * pageSize;
        const items = list.slice(start, start + pageSize);
        const body = {
            count: items.length,
            total,
            page,
            pageSize,
            _links: {
                self: { href: `${base}/instruments?page=${page}&pageSize=${pageSize}${sort ? `&sort=${sort}` : ''}` },
                create: { href: `${base}/instruments`, method: 'POST' }
            },
            _embedded: items.map(i => hateoasInstrument(base, i))
        };
        await cacheSet(cacheKey, body, 30);
        res.set('X-Cache-Status', 'MISS');
        return res.json(body);
    }
    catch (e) {
        console.error('SOAP error on GET list:', e);
        return res.status(502).json({ error: 'bad_gateway', message: 'SOAP service error' });
    }
}); // Explicitly handle trailing slash as 404 (doesnt work aaaa)
router.get('/instruments/', requireScope('read'), async (_req, res) => {
    return res.status(404).json({ error: 'not_found' });
});
router.get('/instruments/:id', requireScope('read'), async (req, res) => {
    const id = Number(req.params.id);
    if (!Number.isFinite(id) || id <= 0)
        return res.status(400).json({ error: 'bad_request', message: 'id must be a positive integer' });
    try {
        const itemKey = `item:v1:${id}`;
        const cached = await cacheGet(itemKey);
        if (cached) {
            const base = `${req.protocol}://${req.get('host')}`;
            res.set('X-Cache-Status', 'HIT');
            return res.json(hateoasInstrument(base, cached));
        }
        const it = await soapGet(id);
        if (!it)
            return res.status(404).json({ error: 'not_found', message: `Instrument with id ${id} not found` });
        const base = `${req.protocol}://${req.get('host')}`;
        await cacheSet(itemKey, it, 60);
        res.set('X-Cache-Status', 'MISS');
        return res.json(hateoasInstrument(base, it));
    }
    catch (e) {
        console.error('SOAP error on GET by id:', e);
        return res.status(502).json({ error: 'bad_gateway', message: 'SOAP service error' });
    }
});
router.post('/instruments', requireScope('write'), async (req, res) => {
    console.log('POST /instruments called');
    console.log('Headers:', req.headers);
    console.log('Body:', req.body);
    console.log('Raw body:', req.body ? JSON.stringify(req.body) : 'No body');
    const { error, value } = instrumentSchema.validate(req.body, { abortEarly: false });
    if (error) {
        console.log('Validation error:', error.details.map(d => d.message));
        return res.status(400).json({ error: 'validation', details: error.details.map(d => d.message) });
    }
    try {
        // Duplicate detection by nombre
        const existing = await soapList();
        const norm = (s) => (s || '').trim().toLowerCase();
        const dup = existing.find(i => norm(i.nombre) === norm(value.nombre));
        if (dup) {
            return res.status(409).json({ error: 'conflict', message: 'An instrument with this nombre already exists', id: dup.id });
        }
        const created = await soapCreate(value);
        // bump list cache version 
        await cacheBump('list');
        const base = `${req.protocol}://${req.get('host')}`;
        res.setHeader('Location', `${base}/instruments/${created.id}`);
        return res.status(201).json(hateoasInstrument(base, created));
    }
    catch (e) {
        console.error('SOAP error on POST:', e);
        return res.status(424).json({ error: 'failed_dependency', message: 'SOAP service failed to create' });
    }
});
router.put('/instruments/:id', requireScope('write'), async (req, res) => {
    const id = Number(req.params.id);
    if (!Number.isFinite(id) || id <= 0)
        return res.status(400).json({ error: 'bad_request', message: 'id must be a positive integer' });
    try {
        // Check if nombre already exists in another instrument
        if (req.body.nombre) {
            const existing = await soapList();
            const dup = existing.find(i => i.id !== id && i.nombre === req.body.nombre);
            if (dup) {
                return res.status(409).json({ error: 'conflict', message: 'An instrument with this nombre already exists', id: dup.id });
            }
        }
        const updated = await soapUpdate(id, req.body || {});
        if (!updated)
            return res.status(404).json({ error: 'not_found', message: `Instrument with id ${id} not found` });
        await cacheDel(`item:v1:${id}`);
        await cacheBump('list');
        const base = `${req.protocol}://${req.get('host')}`;
        return res.json(hateoasInstrument(base, updated));
    }
    catch (e) {
        console.error('SOAP error on PUT:', e);
        return res.status(424).json({ error: 'failed_dependency', message: 'SOAP service failed to update' });
    }
});
// PATCH
router.patch('/instruments/:id', requireScope('write'), async (req, res) => {
    const id = Number(req.params.id);
    if (!Number.isFinite(id) || id <= 0)
        return res.status(400).json({ error: 'bad_request', message: 'id must be a positive integer' });
    try {
        // Check if nombre already exists in another instrument
        if (req.body.nombre) {
            const existing = await soapList();
            const dup = existing.find(i => i.id !== id && i.nombre === req.body.nombre);
            if (dup) {
                return res.status(409).json({ error: 'conflict', message: 'An instrument with this nombre already exists', id: dup.id });
            }
        }
        const updated = await soapUpdate(id, req.body || {});
        if (!updated)
            return res.status(404).json({ error: 'not_found', message: `Instrument with id ${id} not found` });
        await cacheDel(`item:v1:${id}`);
        await cacheBump('list');
        const base = `${req.protocol}://${req.get('host')}`;
        return res.json(hateoasInstrument(base, updated));
    }
    catch (e) {
        console.error('SOAP error on PATCH:', e);
        return res.status(424).json({ error: 'failed_dependency', message: 'SOAP service failed to update' });
    }
});
router.delete('/instruments/:id', requireScope('write'), async (req, res) => {
    const id = Number(req.params.id);
    if (!Number.isFinite(id) || id <= 0)
        return res.status(400).json({ error: 'bad_request', message: 'id must be a positive integer' });
    try {
        const ok = await soapDelete(id);
        if (!ok)
            return res.status(404).json({ error: 'not_found', message: `Instrument with id ${id} not found` });
        await cacheDel(`item:v1:${id}`);
        await cacheBump('list');
        return res.status(204).send();
    }
    catch (e) {
        console.error('SOAP error on DELETE:', e);
        return res.status(424).json({ error: 'failed_dependency', message: 'SOAP service failed to delete' });
    }
});
export default router;
