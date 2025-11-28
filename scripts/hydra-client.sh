#!/bin/sh
set -eux

echo "Intentando importar cliente con la CLI de Hydra (reintentos)..."
for i in $(seq 1 60); do
  set +e
  hydra import oauth2-client /clients/clients.json --endpoint http://hydra:4445
  RC=$?
  set -e
  if [ "$RC" -eq 0 ]; then
    echo "Importado cliente"
    break
  fi
  echo "Intento $i fallo (RC=$RC), esperando..."
  sleep 2
done

# Verify
if hydra get oauth2-client --endpoint http://hydra:4445 musicshop >/dev/null 2>&1; then
  echo "Cliente OK"
  exit 0
fi

echo "Error verificando cliente con la CLI"
exit 1
