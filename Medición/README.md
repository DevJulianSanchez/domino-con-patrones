# Medición de recursos y tiempo (Anexo C)

Mediciones del modo banco de pruebas de `Domino.Simulacion`: partidas completas sin interfaz,
semilla fija, sin imprimir dentro del bucle medido. El análisis está en la sección
«Medición de recursos y tiempo» del documento de diseño.

## Entorno

| Dato | Valor |
|---|---|
| Máquina | MacBook Pro, Apple M1 Pro (6 núcleos de rendimiento y 2 de eficiencia), 16 GB |
| Sistema operativo | macOS 27.2.1 |
| Plataforma | .NET SDK 10.0.102, runtime 10.0.2, Arm64, GC de estación de trabajo concurrente |
| Compilación | `Release` |
| Perfiladores | `dotnet-trace` y `dotnet-counters` 10.0.750501 |

## Escenario

Doble 9, humano simulado y tres CPU (Principiante, Intermedio y Avanzado), sin límite de tiempo.
El humano juega la primera opción válida; tras su tercera acción deshace, guarda y carga. Es el
mismo escenario de `SimuladorPartidas`: el banco reproduce sus victorias (296, 252, 229, 215 y
8 empates con semilla 42), lo que comprueba que mide las mismas partidas.

## Protocolo

1. Cada proceso descarta 200 partidas de calentamiento y luego ejecuta 3 corridas de 1000
   partidas con semilla 42; reporta la mediana de las corridas.
2. Cada etapa ejecuta 5 procesos independientes y `resumir_banco.py` toma la mediana entre
   procesos (en Apple Silicon el hilo puede caer en núcleos de distinto tipo entre procesos).
3. Por invocación se toma el tiempo de reloj (`Stopwatch`, resolución de 41,7 ns), el tiempo de
   CPU del hilo (`clock_gettime(CLOCK_THREAD_CPUTIME_ID)`, resolución de 1 µs) y los bytes
   asignados por el hilo. Se reportan p50 y p95, no el promedio.
4. El dominio no se instrumenta: toda la medición está en `BancoPruebas`.

## Cómo reproducir

Desde la raíz del repositorio:

```sh
sh Medición/medir.sh <etapa>      # 5 procesos + resumen en Medición/<etapa>/

# Contadores del runtime (asignación, colecciones y pausas de GC, CPU)
dotnet-counters collect --counters System.Runtime --format csv -o Medición/<etapa>/contadores.csv \
  -- dotnet Código/src/Domino.Simulacion/bin/Release/net10.0/Domino.Simulacion.dll banco 1000 42 5
python3 Medición/resumir_contadores.py Medición/<etapa>/contadores.csv

# Perfil de CPU de la simulación normal (a través de ControladorJuego, sin medición manual)
dotnet-trace collect --profile dotnet-sampled-thread-time -o Medición/<etapa>/simulacion.nettrace \
  -- dotnet Código/src/Domino.Simulacion/bin/Release/net10.0/Domino.Simulacion.dll 3000 42
dotnet-trace report Medición/<etapa>/simulacion.nettrace topN -n 45 --inclusive
```

## Etapas

| Carpeta | Código medido |
|---|---|
| `linea_base/` | Código de la Entrega 3 antes de optimizar (`OpcionesSobreMesa` con LINQ, JSON con sangría). |
| `mejora1/` | `Partida.OpcionesSobreMesa` sin colecciones intermedias. |
| `mejora2/` | `RepositorioPartidasJson` lee y escribe UTF-8 directamente y sin sangría. |

Cada carpeta tiene `proceso<i>.csv` (un proceso), `banco.csv` y `resumen.txt` (mediana entre
procesos). `linea_base` y `mejora2` tienen además `contadores.csv` y la grabación `.nettrace`.
