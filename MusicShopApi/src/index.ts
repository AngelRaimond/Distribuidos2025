import express from 'express';
import instruments from './routes/instruments.js';
import sellers from './routes/sellers.js';
import { authMiddleware } from './lib/hydra.js';
import SellerGrpcClient from './lib/grpc-seller.js';

const app = express();
app.use(express.json({ limit: '1mb' }));

// Enforce strict routing so trailing slashes are not treated as equivalent
// This makes '/instruments/' not match '/instruments' and return 404 as expected by tests
app.set('strict routing', true);

app.use((req, res, next) => {
  console.log(`${req.method} ${req.url}`);
  console.log('Headers:', req.headers);
  next();
});

// Check gRPC connection on startup
SellerGrpcClient.healthCheck().then(ready => {
  if (!ready) {
    console.warn('WARNING: SellerApi gRPC service is not available');
  }
});

// Auth obligatorio en todas las rutas
app.use(authMiddleware);

app.use(instruments);
app.use('/api/sellers', sellers);

const port = 8080;
app.listen(port, () => console.log(`MusicShopApi listening on ${port}`));
