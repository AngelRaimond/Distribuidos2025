# MusicShopApi 

Descripción 
-----------------
MusicShopApi es un  REST (Node + TypeScript + Express) que consume el proveedor SOAP `InstrumentosAPI`. Expone endpoints REST para listar, obtener, crear, actualizar y borrar instrumentos y añade HATEOAS, paginación, cache y autorización.

Tecnologías 
----------------------
- Node.js + TypeScript
- Express
- Axios (cliente HTTP hacia el SOAP provider)
- Joi (validación de payloads)
- ioredis (cache )
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

Cómo ejecutar 
------------------------------------
1. Clona el repositorio 
descomprimelo y navega a la carpeta con cd a la carpeta llamada MusicShopApi
2. Instala dependencias y compila:

```powershell
npm install
npm run build
```


2. levanta el compose con 


```powershell
docker-compose up --build -d
#o
podman compose up --build -d 
```
3. navega a la carpeta raiz del proyecto

Instrucciones completas para probar
----------------------------------

pruebas :D
- El script de pruebas está en `MusicShopApi/scripts/run-tests.ps1`.
- Comando de ejecución (desde la raíz del repositorio):

```powershell
powershell -ExecutionPolicy Bypass -File TU ruta COMPLETA no relativa a  el archivo que se encuentra en \MusicShopApi\scripts\run-tests.ps1
se puede ver algo asi: pero cambia dependiendo de tu ruta al proyecto 
powershell -ExecutionPolicy Bypass -File C:\Users\USER\Desktop\Distribuidos\Distribuidos2025\MusicShopApi\scripts\run-tests.ps1
```

Casos de prueba cubiertos por `run-tests.ps1`
---------------------------------------------
El script ejecuta las siguientes comprobaciones (en orden). Cada punto indica el objetivo y el resultado esperado.

1) POST invalid — cuerpo inválido -> esperar 400 Bad Request
	- Envía un body con campos vacíos/invalidos (nombre vacío, precio negativo, año fuera de rango, categoría vacía).
	- Esperado: 400 (validación de payload).

2) POST create — crear recurso válido -> esperar 201 Created (o 424/502 si falla el SOAP)
	- Crea un instrumento con datos válidos y espera 201. Si el proveedor SOAP falla, se aceptan 424/502.

3) POST duplicate -> esperar 409 Conflict
	- Reintenta crear el mismo recurso; espera conflicto por duplicado.

3b) POST duplicate by nombre -> esperar 409 Conflict
	- Intenta crear un recurso con el mismo `nombre` que otro existente; espera 409.

4) GET list -> esperar 200 OK con paginación
	- Solicita `/instruments?page=1&pageSize=5&sort=nombre` y valida la forma de la respuesta y cabeceras de cache.

5) GET by existing id -> esperar 200 OK (prueba también cache HIT)
	- Recupera un id creado previamente y realiza una segunda petición para comprobar cabecera `X-Cache-Status`.

6) GET by missing id -> esperar 404 Not Found
	- Solicita un id inexistente y espera 404.

7) Create second instrument -> preparar datos para tests de duplicados
	- Crea un segundo recurso para usar su `id` y `nombre` en tests posteriores.

8) PUT replace -> esperar 200 o 204
	- Reemplaza completamente un recurso (PUT) y acepta 200 o 204 

9) PUT with duplicate nombre -> esperar 409 Conflict
	- Intenta actualizar un recurso con un `nombre` que ya existe en otro recurso; espera 409.

10) PATCH partial update -> esperar 200 o 204
	- Actualización parcial (PATCH) de un campo (ej. `precio`) y espera 200/204.

11) PATCH with duplicate nombre -> esperar 409 Conflict
	- Actualiza parcialmente el `nombre` para duplicarlo y espera 409.

12) DELETE first instrument -> esperar 204 No Content (o 200)
	- Borra el primer recurso creado; acepta 204 o 200

13) DELETE second instrument -> esperar 204 No Content (o 200)
	- Borra el segundo recurso creado; acepta 204 o 200.

14) DELETE missing -> esperar 404 
	- Intenta borrar un id inexistente; se acepta 404

15) GET without token -> esperar 401 Unauthorized (o 403)
	- Solicita la lista sin cabecera Authorization; espera 401 o 403.



