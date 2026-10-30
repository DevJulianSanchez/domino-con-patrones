# Documento de diseño · Dominó con patrones

Proyecto LaTeX del documento de diseño del curso Diseño Basado en Patrones (UAN, 2026-II),
preparado para trabajar en VS Code.

## Requisitos

- Distribución LaTeX con XeLaTeX, BibTeX y latexmk (TeX Live o MiKTeX).
- draw.io de escritorio (lo usa `.latexmkrc` para exportar los diagramas a PDF vectorial).
  Si está en una ruta no estándar, definir la variable de entorno `DRAWIO`.
- VS Code con las extensiones recomendadas (`.vscode/extensions.json` en la raíz del repositorio):
  - **LaTeX Workshop** (`james-yu.latex-workshop`)
  - **Draw.io Integration** (`hediet.vscode-drawio`)

## Compilar y ver el PDF en vivo

1. Abrir en VS Code la raíz del repositorio (`Proyecto final`) y abrir `Documentación/main.tex`.
2. **LaTeX Workshop: View LaTeX PDF** (icono de vista previa o `⌥⌘V`): el PDF se abre en una
   pestaña a la derecha.
3. Desde ahí, cualquier cambio guardado en `.tex`, `.sty`, `.bib` **o `.drawio`** recompila
   y refresca el PDF (`autoBuild.run = onFileChange`). Con `⌘`+clic en el PDF se salta a la
   línea del fuente (SyncTeX).

El PDF queda en `build/main.pdf`. Desde terminal: `latexmk main.tex`.

## Estructura

| Ruta | Contenido |
|---|---|
| `main.tex` | Portada, índices y orden de las secciones |
| `generado/datos.tex` | Metadatos: autor, docente, versión, repositorio, plataforma |
| `config/estilo.sty` | Formato: tipografía Pagella, colores UAN, tablas, plantilla de caso de uso |
| `config/logo_uan.png` | Logotipo institucional para la portada |
| `secciones/` | Un archivo por sección del documento |
| `diagramas/` | Diagramas editables en draw.io |
| `referencias.bib` | Referencias, citables por clave desde cualquier sección |

## Diagramas (draw.io en VS Code)

| Archivo | Sección donde se incluye |
|---|---|
| `diagramas/casos_uso.drawio` | 4. Casos de uso |
| `diagramas/modelo_dominio.drawio` | 5. Modelo de dominio |
| `diagramas/enumeraciones.drawio` | 5.2. Valores enumerados |
| `diagramas/diagrama_clases.drawio` | 6. Diseño de clases: entidades e interfaces (página apaisada) |
| `diagramas/tipos_dato.drawio` | 6. Diseño de clases: tipos de valor |
| `diagramas/patron_strategy.drawio` | 8. Patrones: ficha de Strategy (antes y después) |
| `diagramas/patron_state.drawio` | 8. Patrones: ficha de State (antes y después) |
| `diagramas/estados_partida.drawio` | 8. Patrones: diagrama de estados de la partida |
| `diagramas/patron_command.drawio` | 8. Patrones: ficha de Command (antes y después) |

Flujo:

1. Abrir el `.drawio` en VS Code: se edita con el editor de draw.io embebido.
   Se puede dividir el editor y tener el diagrama a la izquierda y el PDF a la derecha.
2. Guardar (`⌘S`). En la siguiente compilación `.latexmkrc` exporta a
   `diagramas/exportados/<nombre>.pdf` solo los diagramas que cambiaron, y el PDF se refresca.
3. Para un diagrama nuevo: crear `diagramas/<nombre>.drawio` (una sola página) e incluirlo con
   `\diagrama{<nombre>}{Título}{fig:etiqueta}`, o `\diagrama*{...}` para página apaisada.
   Ancho opcional: `\diagrama[0.8\textwidth]{...}`.

Los PDF de `diagramas/exportados/` se versionan junto con los `.drawio`, para que el documento
compile también en un equipo sin draw.io. Si el PDF no existe, el documento muestra un recuadro
`pendiente` en lugar de la figura.

## Macros útiles

- `\citar{clave}`: cita en negrita (autor, año).
- `\diagrama[ancho]{nombre}{título}{etiqueta}` y `\diagrama*{...}`: figura desde draw.io.
- `\begin{pendiente} ... \end{pendiente}`: marca contenido pendiente; no debe quedar ninguno al entregar.
- `\encabezadoCU`, `\cajaCU`, `cursonormal`, `cursosalternos`, `\otrosdatosCU`:
  plantilla de caso de uso del curso (ver `secciones/05_casos_uso.tex`).
- `\cab{Texto}` y `\filasep`: encabezado y separador de filas en tablas.

## Cada nueva entrega

1. Agregar una fila en `secciones/00_control_cambios.tex`.
2. Actualizar `\Entrega`, `\VersionDoc` y `\FechaEntrega` en `generado/datos.tex`.
3. Nombre del PDF a subir: `Julian_F_Sanchez_T_DBP_E<n>_<AAAAMMDD>_documento_diseno.pdf`.
