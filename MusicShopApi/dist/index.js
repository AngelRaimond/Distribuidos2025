import express from 'express';
import instruments from './routes/instruments.js';
import { authMiddleware } from './lib/hydra.js';
const app = express();
app.use(express.json({ limit: '1mb' }));
app.set('strict routing', true);
app.use((req, res, next) => {
    console.log(`${req.method} ${req.url}`);
    console.log('Headers:', req.headers);
    next();
});
// Auth obligatorio en todas las rutas
app.use(authMiddleware);
app.use(instruments);
const port = 8080;
app.listen(port, () => console.log(`MusicShopApi listening on ${port}`));
