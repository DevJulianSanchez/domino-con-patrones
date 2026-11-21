"""Resume el CSV de dotnet-counters: totales de GC, asignación, pausas y CPU de la corrida."""
import csv
import sys
from collections import defaultdict

sumas = defaultdict(float)
maximos = defaultdict(float)
with open(sys.argv[1], newline="") as archivo:
    for fila in csv.DictReader(archivo):
        nombre, valor = fila["Counter Name"], float(fila["Mean/Increment"] or 0)
        sumas[nombre] += valor
        maximos[nombre] = max(maximos[nombre], valor)

def total(clave):
    return sum(v for k, v in sumas.items() if k.startswith(clave))

print(f"Colecciones gen0/gen1/gen2: {sumas['dotnet.gc.collections ({collection} / 1 sec)[gc.heap.generation=gen0]']:.0f} / "
      f"{sumas['dotnet.gc.collections ({collection} / 1 sec)[gc.heap.generation=gen1]']:.0f} / "
      f"{sumas['dotnet.gc.collections ({collection} / 1 sec)[gc.heap.generation=gen2]']:.0f}")
print(f"Bytes asignados: {total('dotnet.gc.heap.total_allocated') / 2**20:.1f} MiB")
print(f"Pausa total de GC: {total('dotnet.gc.pause.time') * 1000:.1f} ms")
print(f"CPU usuario + sistema: {total('dotnet.process.cpu.time') :.2f} s")
print(f"Working set máximo: {maximos['dotnet.process.memory.working_set (By)'] / 2**20:.1f} MiB")
