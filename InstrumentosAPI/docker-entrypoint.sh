#!/bin/sh
set -e
echo "PWD=$(pwd)"
if [ -f ./run.rb ]; then
  echo "Launching SOAP: ./run.rb"
  exec ruby ./run.rb
else
  echo "ERROR: no se encontró run.rb" >&2
  ls -R >&2
  exit 1
fi
