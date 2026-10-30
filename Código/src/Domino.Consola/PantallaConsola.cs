using System.Text;
using Domino.Dominio;

namespace Domino.Consola;

/// <summary>
/// Dibuja el juego en una sola pantalla que se redibuja en su lugar (pantalla alterna de la
/// terminal), con las fichas representadas por sus puntos. Solo presenta: no lee teclas ni
/// conoce el controlador.
/// </summary>
public sealed class PantallaConsola
{
    private const string Normal = "\e[0m";
    private const string Negrita = "\e[1m";
    private const string Tenue = "\e[2m";
    private const string Invertido = "\e[7m";
    private const string Amarillo = "\e[33m";
    private const string Cian = "\e[36m";
    private const string Rojo = "\e[31m";
    private const string Verde = "\e[32m";

    /// <summary>Posiciones de la cuadrícula de 3×3 (por filas) que lleva punto en cada valor de 0 a 9.</summary>
    private static readonly int[][] Puntos =
    [
        [], [4], [2, 6], [2, 4, 6], [0, 2, 6, 8], [0, 2, 4, 6, 8],
        [0, 2, 3, 5, 6, 8], [0, 2, 3, 4, 5, 6, 8], [0, 1, 2, 3, 5, 6, 7, 8], [0, 1, 2, 3, 4, 5, 6, 7, 8]
    ];

    private const int AnchoFichaMesa = 9;
    private const int AnchoFichaMano = 13;

    public void Abrir()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Write("\e[?1049h\e[?25l");
    }

    public void Cerrar() => Console.Write("\e[?25h\e[?1049l");

    public void DibujarMenu(string titulo, IReadOnlyList<string> opciones, int seleccion, IReadOnlyList<string> resumen, string mensaje = "")
    {
        var lineas = Encabezado();
        foreach (var linea in resumen)
            lineas.Add($"  {Tenue}{linea}{Normal}");
        if (resumen.Count > 0)
            lineas.Add("");
        lineas.Add($"  {Negrita}{titulo}{Normal}");
        for (var i = 0; i < opciones.Count; i++)
            lineas.Add(i == seleccion ? $"  {Amarillo}❯ {Invertido} {opciones[i]} {Normal}" : $"     {opciones[i]}");
        lineas.Add("");
        if (opciones.Count > 0)
            lineas.Add($"  {Tenue}↑/↓ moverse · Enter elegir · Esc cancelar{Normal}");
        lineas.Add(Mensaje(mensaje));
        Escribir(lineas);
    }

    public void DibujarTablero(VistaPartida vista, int seleccion, string mensaje)
    {
        var (ancho, alto) = Dimensiones();
        var fijas = new List<string>();
        fijas.AddRange(Encabezado(vista));
        fijas.AddRange(Rivales(vista));

        var jugadas = JugadasRecientes(vista, 4);
        var mano = SeccionMano(vista, seleccion, ancho, grande: true);
        var ayuda = new List<string> { "", Ayuda(vista), Mensaje(mensaje) };

        var disponible = alto - fijas.Count - jugadas.Count - ayuda.Count - 1;
        var mesa = SeccionMesa(vista, ancho, conPuntos: true);
        if (mesa.Count + mano.Count > disponible)
            mesa = SeccionMesa(vista, ancho, conPuntos: false);
        if (mesa.Count + mano.Count > disponible)
            mano = SeccionMano(vista, seleccion, ancho, grande: false);

        Escribir([.. fijas, .. mesa, .. jugadas, .. mano, .. ayuda]);
    }

    public void DibujarFinal(VistaPartida vista)
    {
        var (ancho, alto) = Dimensiones();
        var lineas = Encabezado(vista);
        var mesa = SeccionMesa(vista, ancho, conPuntos: true);
        if (mesa.Count + vista.Jugadores.Count + 15 > alto)
            mesa = SeccionMesa(vista, ancho, conPuntos: false);
        lineas.AddRange(mesa);
        lineas.AddRange(JugadasRecientes(vista, 3));

        var resultado = vista.Resultado!;
        lineas.Add("");
        lineas.Add(resultado.Forma == FormaDeCierre.Domino
            ? $"  {Negrita}{Verde}¡DOMINÓ!{Normal}"
            : $"  {Negrita}{Amarillo}JUEGO CERRADO{Normal} · se revelan las fichas (RN-09)");
        foreach (var p in resultado.Puntajes)
        {
            var marca = p == resultado.Ganador ? $"{Verde}★{Normal}" : " ";
            lineas.Add($"  {marca} {p.Alias,-24} {p.Puntos,3} pts  {Tenue}{string.Join(" ", p.Fichas)}{Normal}");
        }
        lineas.Add("");
        lineas.Add(resultado.EsEmpate
            ? $"  {Negrita}Partida empatada.{Normal}"
            : $"  {Negrita}Gana {resultado.Ganador!.Alias}.{Normal}");
        lineas.Add("");
        lineas.Add($"  {Tenue}Presiona cualquier tecla para salir.{Normal}");
        Escribir(lineas);
    }

    /// <summary>Lee un texto en la última línea de la pantalla actual.</summary>
    public string LeerTexto(string etiqueta)
    {
        Console.Write($"  {Negrita}{etiqueta}{Normal}\e[?25h");
        var texto = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("\e[?25l");
        return texto;
    }

    // ---------- Secciones ----------

    private static List<string> Encabezado(VistaPartida? vista = null)
    {
        var detalle = vista is null
            ? ""
            : $"   {Tenue}{vista.Estado} · fichas para robar: {vista.FichasParaRobar}{Normal}";
        return [$"  {Negrita}{Cian}▣ DOMINÓ CON PATRONES{Normal}{detalle}", $"  {Tenue}{new string('─', 60)}{Normal}"];
    }

    private static List<string> Rivales(VistaPartida vista)
    {
        var lineas = new List<string>();
        for (var i = 0; i < vista.Jugadores.Count; i++)
        {
            if (i == vista.PosicionPropia)
                continue;
            var fichas = vista.FichasPorJugador[i];
            lineas.Add($"  {vista.Jugadores[i],-24} {Tenue}{new string('▮', fichas)}{Normal} {fichas}");
        }
        lineas.Add("");
        return lineas;
    }

    private static List<string> SeccionMesa(VistaPartida vista, int ancho, bool conPuntos)
    {
        var extremos = vista.ExtremoIzquierdo is { } izq && vista.ExtremoDerecho is { } der
            ? $"   {Tenue}◀ {Normal}{Negrita}{izq}{Normal}{Tenue} … {Normal}{Negrita}{der}{Normal}{Tenue} ▶{Normal}"
            : "";
        var lineas = new List<string> { $"  {Negrita}MESA{Normal}{extremos}" };

        if (vista.Mesa.Count == 0)
            lineas.Add($"  {Tenue}(vacía){Normal}");
        else if (conPuntos)
        {
            var reciente = UltimaFichaColocada(vista);
            var fichas = vista.Mesa
                .Select(f => DibujarFicha(f.ValorIzquierdo, f.ValorDerecho, grande: false, f.Ficha == reciente ? Cian : ""))
                .ToList();
            lineas.AddRange(EnFilas(fichas, AnchoFichaMesa, ancho));
        }
        else
            lineas.AddRange(EnFilasDeTexto(vista.Mesa.Select(f => $"[{f.ValorIzquierdo}|{f.ValorDerecho}]"), ancho));

        lineas.Add("");
        return lineas;
    }

    private static List<string> SeccionMano(VistaPartida vista, int seleccion, int ancho, bool grande)
    {
        var tiempo = vista.SegundosRestantes is { } s ? $"   {(s <= 3 ? Rojo : Amarillo)}⏱ {s} s{Normal}" : "";
        var lineas = new List<string> { $"  {Negrita}TU MANO{Normal} {Tenue}({vista.Jugadores[vista.PosicionPropia]}){Normal}{tiempo}" };

        var jugables = vista.Opciones.Select(o => o.Ficha).ToHashSet();
        var fichas = vista.ManoPropia.Select((f, i) =>
        {
            var estilo = (i == seleccion ? Invertido : "") + (jugables.Contains(f) ? Negrita : Tenue);
            return DibujarFicha(f.ValorA, f.ValorB, grande, estilo);
        }).ToList();
        var anchoFicha = grande ? AnchoFichaMano : AnchoFichaMesa;

        // Debajo de cada fila de fichas, una flecha señala la seleccionada.
        var porFila = FichasPorFila(anchoFicha, ancho);
        var filas = EnFilas(fichas, anchoFicha, ancho);
        for (var fila = 0; fila * porFila < fichas.Count; fila++)
        {
            lineas.AddRange(filas.Skip(fila * 5).Take(5));
            var inicio = fila * porFila;
            var marcas = Enumerable.Range(inicio, Math.Min(porFila, fichas.Count - inicio))
                .Select(i => (i == seleccion ? "▲" : " ").PadLeft(anchoFicha / 2 + 1).PadRight(anchoFicha + 1));
            lineas.Add($"  {Amarillo}{string.Concat(marcas)}{Normal}");
        }
        return lineas;
    }

    private static List<string> JugadasRecientes(VistaPartida vista, int cantidad)
    {
        var lineas = new List<string> { $"  {Negrita}ÚLTIMAS JUGADAS{Normal}" };
        var recientes = vista.UltimasJugadas.TakeLast(cantidad).ToList();
        if (recientes.Count == 0)
            lineas.Add($"  {Tenue}(ninguna){Normal}");
        foreach (var jugada in recientes)
        {
            var quien = jugada.PosicionJugador == vista.PosicionPropia ? "Tú" : vista.Jugadores[jugada.PosicionJugador];
            lineas.Add($"  {Tenue}·{Normal} {quien}: {Describir(jugada)}");
        }
        lineas.Add("");
        return lineas;
    }

    private static string Ayuda(VistaPartida vista)
    {
        var acciones = new List<string> { "←/→ ficha", "Enter jugar" };
        if (vista.PuedeRobar) acciones.Add($"{Amarillo}R robar{Normal}{Tenue}");
        if (vista.PuedePasar) acciones.Add($"{Amarillo}P pasar{Normal}{Tenue}");
        if (vista.PuedeDeshacer) acciones.Add("D deshacer");
        acciones.Add("G guardar");
        acciones.Add("S salir");
        return $"  {Tenue}{string.Join(" · ", acciones)}{Normal}";
    }

    private static string Mensaje(string mensaje) =>
        mensaje.Length == 0 ? "" : $"  {Amarillo}{mensaje}{Normal}";

    private static string Describir(Jugada jugada) => jugada switch
    {
        JugadaColocarFicha c => $"coloca {c.Ficha} {(c.Extremo == Extremo.Izquierdo ? "a la izquierda" : "a la derecha")}",
        JugadaRobar => "roba una ficha",
        JugadaPasar => "pasa",
        JugadaTiempoAgotado => $"{Rojo}pierde el turno por tiempo{Normal}",
        _ => jugada.Tipo.ToString()
    };

    private static Ficha? UltimaFichaColocada(VistaPartida vista) =>
        vista.UltimasJugadas.OfType<JugadaColocarFicha>().LastOrDefault()?.Ficha;

    // ---------- Dibujo de fichas ----------

    /// <summary>Ficha de 5 líneas con los puntos de cada mitad en una cuadrícula de 3×3.</summary>
    private static string[] DibujarFicha(int izquierdo, int derecho, bool grande, string estilo)
    {
        var mitad = grande ? 5 : 3;
        var borde = new string('─', mitad);
        var lineas = new string[5];
        lineas[0] = $"┌{borde}┬{borde}┐";
        for (var fila = 0; fila < 3; fila++)
            lineas[fila + 1] = $"│{FilaDePuntos(izquierdo, fila, grande)}│{FilaDePuntos(derecho, fila, grande)}│";
        lineas[4] = $"└{borde}┴{borde}┘";
        return lineas.Select(l => $"{estilo}{l}{Normal}").ToArray();
    }

    private static string FilaDePuntos(int valor, int fila, bool grande)
    {
        var celdas = Enumerable.Range(fila * 3, 3).Select(c => Puntos[valor].Contains(c) ? "●" : " ");
        return string.Join(grande ? " " : "", celdas);
    }

    private static int FichasPorFila(int anchoFicha, int ancho) => Math.Max(1, (ancho - 2) / (anchoFicha + 1));

    private static List<string> EnFilas(IReadOnlyList<string[]> fichas, int anchoFicha, int ancho)
    {
        var porFila = FichasPorFila(anchoFicha, ancho);
        var lineas = new List<string>();
        foreach (var grupo in fichas.Chunk(porFila))
            for (var l = 0; l < 5; l++)
                lineas.Add("  " + string.Join(" ", grupo.Select(f => f[l])));
        return lineas;
    }

    private static List<string> EnFilasDeTexto(IEnumerable<string> fichas, int ancho)
    {
        var lineas = new List<string>();
        var actual = new StringBuilder("  ");
        foreach (var ficha in fichas)
        {
            if (actual.Length + ficha.Length + 1 > ancho)
            {
                lineas.Add(actual.ToString());
                actual.Clear().Append("  ");
            }
            actual.Append(ficha).Append(' ');
        }
        lineas.Add(actual.ToString());
        return lineas;
    }

    // ---------- Terminal ----------

    private static (int Ancho, int Alto) Dimensiones()
    {
        try
        {
            return (Math.Max(40, Console.WindowWidth), Math.Max(20, Console.WindowHeight));
        }
        catch (IOException)
        {
            return (100, 40);
        }
    }

    /// <summary>Redibuja la pantalla completa en su lugar, sin desplazar ni parpadear.</summary>
    private static void Escribir(IEnumerable<string> lineas)
    {
        var marco = new StringBuilder("\e[H");
        foreach (var linea in lineas)
            marco.Append(linea).Append("\e[K\n");
        marco.Append("\e[J");
        Console.Write(marco.ToString());
    }
}
