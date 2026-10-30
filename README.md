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
dotnet run --project src/Domino.Simulacion -- 1000 42   # partidas, semilla
dotnet test                                   # pruebas de reglas y patrones
```

| Proyecto | Depende de | Contenido |
|---|---|---|
| `Domino.Dominio` | — | Reglas del juego, entidades, tipos de valor y abstracciones |
| `Domino.Persistencia` | Dominio | `RepositorioPartidasJson` |
| `Domino.Consola` | Dominio, Persistencia | `InterfazConsola`, `RelojSistema` |
| `Domino.Simulacion` | Dominio, Persistencia | `SimuladorPartidas`, `RelojSimulado` |
| `tests/Domino.Pruebas` | Dominio, Persistencia | Pruebas xUnit de las reglas y de los patrones |

Los nombres de clases y miembros coinciden con el diagrama de clases del documento
(`Documentación/diagramas/diagrama_clases.drawio` y `tipos_dato.drawio`).

## Documento

Abrir esta carpeta en VS Code, abrir `Documentación/main.tex` y usar *LaTeX Workshop: View LaTeX PDF*.

## Entregas

Cada entrega se marca con una etiqueta (`E1`, `E2`, `E3`) y un release en GitHub con el PDF adjunto.
