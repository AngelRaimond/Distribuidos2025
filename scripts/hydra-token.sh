#!/bin/sh
set -e
apk add --no-cache curl jq >/dev/null 2>&1 || true
echo "Esperando Hydra public (4444)..."
for i in $(seq 1 60); do
  if curl -fsS http://hydra:4444/health/ready >/dev/null 2>&1; then break; fi
  sleep 1
done
RESP=$(curl -fsS -d "grant_type=client_credentials&client_id=musicshop&client_secret=secret&scope=read%20write" http://hydra:4444/oauth2/token)
mkdir -p /tokens
echo "$RESP" > /tokens/token.json
echo "$RESP" | jq -r .access_token > /tokens/access_token || true
if [ ! -s /tokens/access_token ]; then
  echo "Token vacío. Respuesta completa guardada en /tokens/token.json" >&2
  exit 1
fi
echo "Token escrito en /tokens/access_token"
tail -f /dev/null
