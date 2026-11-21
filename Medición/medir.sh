#!/bin/sh
# Mide una etapa: 5 procesos independientes, cada uno con calentamiento y 3 corridas de
# 1000 partidas con semilla 42; luego resume con la mediana entre procesos.
# Uso (desde la raíz del repositorio): sh Medición/medir.sh <carpeta-de-la-etapa>
set -e
ETAPA="Medición/$1"
DLL=Código/src/Domino.Simulacion/bin/Release/net10.0/Domino.Simulacion.dll
dotnet build Código/src/Domino.Simulacion -c Release -v q -nologo > /dev/null
mkdir -p "$ETAPA"
for i in 1 2 3 4 5; do
  dotnet "$DLL" banco 1000 42 3 "$ETAPA/proceso$i.csv" > "$ETAPA/proceso$i.txt"
done
python3 Medición/resumir_banco.py "$ETAPA" | tee "$ETAPA/resumen.txt"
