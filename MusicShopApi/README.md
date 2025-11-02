# MusicShopApi — Resumen rápido y guía de pruebas

Descripción corta
-----------------
MusicShopApi es un gateway REST (Node + TypeScript + Express) que consume el proveedor SOAP `InstrumentosAPI`. Expone endpoints REST para listar, obtener, crear, actualizar y borrar instrumentos y añade HATEOAS, paginación, cache y autorización.

Tecnologías principales
----------------------
- Node.js + TypeScript
- Express
- Axios (cliente HTTP hacia el SOAP provider)
- Joi (validación de payloads)
- ioredis (cache opcional)
- Sinatra + Ruby (InstrumentosAPI — proveedor SOAP separado)

Arquitectura / archivos clave
-----------------------------
- `src/index.ts` — arranque de Express y wiring de middlewares (auth, logging).
- `src/routes/instruments.ts` — rutas REST y mapping HATEOAS.
- `src/lib/soap.ts` — cliente SOAP que construye envelopes y parsea respuestas (usa parsing simple / regex, depende de un XML determinista).
- `src/lib/hydra.ts` — middleware de introspección OAuth (ORY Hydra o mock).
- `src/lib/cache.ts` — capa de cache (ioredis).
- `src/lib/validation.ts` — esquemas Joi para los payloads.

Variables de entorno importantes
-------------------------------
- `SOAP_URL` — URL del proveedor SOAP (por defecto `http://localhost:4567/soap`).
- `HYDRA_INTROSPECTION_URL` — endpoint de introspección (por defecto `http://localhost:4445/oauth2/introspect`).
- `HYDRA_CLIENT_ID` / `HYDRA_CLIENT_SECRET` — credenciales usadas para introspección.
- Enports: la app por defecto escucha en el puerto 8080 (ver `src/index.ts`). En tests o compose puede mapearse a otro puerto (p. ej. 8088).

Cómo ejecutar localmente (desarrollo)
------------------------------------
1. Instala dependencias y compila:

```powershell
cd 'C:\Users\angel\Desktop\Distribuidos\Distribuidos2025\MusicShopApi'
npm install
npm run build
```

2. Ejecuta (dev / local):

```powershell
# (opcional) levantar un mock de introspección si no tienes Hydra
# node mock-introspect.js  
npm start
```

3. Asegúrate de que el proveedor SOAP (`InstrumentosAPI`) esté accesible en `SOAP_URL` (por defecto `http://localhost:4567/soap`).

Probar los endpoints
--------------------
- GET /instruments
- GET /instruments/:id
- POST /instruments
- PUT/PATCH /instruments/:id
- DELETE /instruments/:id

Todas las rutas requieren Authorization header `Bearer <token>` (el middleware hace introspección y espera scopes `read` y/o `write`).

Ejecutar el conjunto de tests locales (script de proyecto)
-------------------------------------------------------
El repositorio incluye un script de testing PowerShell `scripts/run-tests.ps1`. Antes de lanzarlo debes:

1. Asegurar que `MusicShopApi` esté corriendo (npm start o via compose).  
2. Asegurar que `InstrumentosAPI` esté corriendo y accesible en `SOAP_URL`.  
3. Asegurar que el servicio de introspección esté accesible en `HYDRA_INTROSPECTION_URL` o ejecutar un mock (ver abajo).

Luego en PowerShell (desde la raíz del repo):

```powershell
powershell -ExecutionPolicy Bypass -File .\MusicShopApi\scripts\run-tests.ps1
```

Problema común: tests devuelven 401 en lugar de 400
-------------------------------------------------
Si los tests fallan con 401 (autenticación) en una petición que debería validar el body, suele ser porque la introspección de token falló y la petición no llega a la validación.

Soluciones rápidas:
- Ver logs de la app para ver qué imprime `src/lib/hydra.ts` (Token, introspect URL, introspection response).  
- Levantar un mock de introspección si no tienes Hydra: crea `mock-introspect.js` con un endpoint POST `/oauth2/introspect` que devuelva `{ active: true, scope: 'read write', sub: 'test' }` y ejecútalo en el puerto 4445.

Ejemplo rápido de mock (Node):

```javascript
// mock-introspect.js
import express from 'express';
import bodyParser from 'body-parser';
const app = express();
app.use(bodyParser.urlencoded({ extended: true }));
app.post('/oauth2/introspect', (_req, res) => res.json({ active: true, scope: 'read write', sub: 'test-user' }));
app.listen(4445, () => console.log('mock introspect :4445'));
```

Nota sobre parsing SOAP
----------------------
`src/lib/soap.ts` usa parsing basado en expresiones regulares y espera una estructura XML determinista (orden y formato de tags). Si cambias el proveedor SOAP o su salida, el parsing puede romperse — preferible mantener el contrato XML estable o mejorar el parser (ej. usar un XML DOM parser).

Recomendaciones
---------------
- Añadir tests de integración que validen requests/responses XML entre `InstrumentosAPI` y `MusicShopApi`.  
- Si se quiere eliminar la dependencia SOAP a largo plazo, planear migración a una API REST interna y adaptar consumidores.

Contacto rápido
---------------
Para problemas con autenticación o introspección, revisa `src/lib/hydra.ts`. Para parsing SOAP revisa `src/lib/soap.ts`.

Fin.
