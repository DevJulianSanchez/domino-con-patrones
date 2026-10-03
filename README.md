# Dominó con patrones

Proyecto semestral de Diseño Basado en Patrones (UAN, Maestría en Arquitectura de Software, 2026-II).

| Carpeta | Contenido |
|---|---|
| `Código/` | Solución .NET 10 (`Domino.slnx`) |
| `Documentación/` | Documento de diseño en LaTeX y diagramas draw.io (ver su README) |

## Código

```bash
cd Código
dotnet build                                  # compila la solución
dotnet run --project src/Domino.Consola       # juego en consola
```

| Proyecto | Depende de | Contenido |
|---|---|---|
| `Domino.Dominio` | — | Reglas del juego, entidades y tipos de valor |
| `Domino.Consola` | Dominio | `InterfazConsola` |

El código es un esqueleto inicial: la consola configura la partida (CU-01) y crea
los jugadores; el reparto y el ciclo de turnos están pendientes.

## Documento

Abrir esta carpeta en VS Code, abrir `Documentación/main.tex` y usar *LaTeX Workshop: View LaTeX PDF*.

## Entregas

Cada entrega se marca con una etiqueta (`E1`, `E2`, `E3`) y un release en GitHub con el PDF adjunto.
