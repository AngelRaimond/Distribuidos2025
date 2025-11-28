#!/bin/sh
set -e
apk add --no-cache curl jq >/dev/null 2>&1 || true
echo "Esperando Hydra admin (4445)..."
for i in $(seq 1 60); do
  if curl -fsS http://hydra:4445/health/ready >/dev/null 2>&1; then break; fi
  sleep 1
done
echo "Upsert client 'musicshop' (client_secret_post)..."
curl -fsS -X PUT http://hydra:4445/clients/musicshop   -H "Content-Type: application/json"   -d '{"client_id":"musicshop","client_secret":"secret","grant_types":["client_credentials"],"scope":"read write","token_endpoint_auth_method":"client_secret_post"}'   >/dev/null 2>&1 || true
curl -fsS http://hydra:4445/clients/musicshop || true
