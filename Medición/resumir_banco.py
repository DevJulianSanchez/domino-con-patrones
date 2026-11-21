"""Mediana, entre los procesos de una etapa, de cada cifra del banco de pruebas."""
import csv
import glob
import statistics
import sys

etapa = sys.argv[1]
filas = {}
for ruta in sorted(glob.glob(f"{etapa}/proceso*.csv")):
    with open(ruta, newline="") as archivo:
        for fila in csv.DictReader(archivo):
            filas.setdefault(fila["accion"], []).append(fila)

columnas = ["p50_us", "p95_us", "cpu_reloj", "bytes_p50", "reloj_total_ms"]
salida = [["accion", "invocaciones"] + columnas]
print(f"{'Acción':34}{'n':>8}{'p50 µs':>9}{'p95 µs':>9}{'CPU/rel':>9}{'B p50':>8}{'total ms':>10}")
for accion, muestras in filas.items():
    valores = []
    for c in columnas:
        datos = [float(m[c]) for m in muestras if m[c]]
        valores.append(statistics.median(datos) if datos else None)
    n = muestras[0]["invocaciones"]
    salida.append([accion, n] + ["" if v is None else f"{v:.2f}" for v in valores])
    texto = ["" if v is None else f"{v:.2f}" for v in valores]
    print(f"{accion:34}{n:>8}{texto[0]:>9}{texto[1]:>9}{texto[2]:>9}{texto[3]:>8}{texto[4]:>10}")

with open(f"{etapa}/banco.csv", "w", newline="") as archivo:
    csv.writer(archivo).writerows(salida)
