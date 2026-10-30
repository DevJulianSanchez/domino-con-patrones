using Domino.Dominio;

namespace Domino.Consola;

/// <summary>
/// Interfaz de texto: traduce las teclas del jugador en operaciones del controlador y entrega
/// las vistas a la pantalla para dibujarlas. No contiene reglas del juego.
/// </summary>
public sealed class InterfazConsola
{
    private readonly ControladorJuego controlador;
    private readonly PantallaConsola pantalla = new();

    public InterfazConsola(ControladorJuego controlador)
    {
        this.controlador = controlador;
    }

    public void Ejecutar()
    {
        pantalla.Abrir();
        Console.CancelKeyPress += (_, _) => pantalla.Cerrar();
        string despedida;
        try
        {
            var vista = Jugar(IniciarOCargar());
            despedida = vista.Resultado is { } r
                ? r.EsEmpate ? "Partida empatada." : $"Gana {r.Ganador!.Alias}."
                : "Partida abandonada.";
        }
        catch (OperationCanceledException)
        {
            despedida = "Configuración cancelada.";
        }
        finally
        {
            pantalla.Cerrar();
        }
        Console.WriteLine(despedida);
    }

    private VistaPartida IniciarOCargar()
    {
        var guardadas = controlador.ListarPartidasGuardadas();
        if (guardadas.Count > 0 && Elegir("¿Qué deseas hacer?", ["Nueva partida", "Cargar partida"], "Nueva partida", []) == "Cargar partida")
        {
            var mensaje = "";
            while (true)
            {
                var nombre = Elegir("Partida guardada", guardadas, guardadas[0], [], mensaje);
                try
                {
                    return controlador.CargarPartida(nombre);
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException or InvalidOperationException or ArgumentException)
                {
                    mensaje = $"No se pudo cargar: {ex.Message}";
                }
            }
        }

        return controlador.IniciarPartida(LeerConfiguracion());
    }

    /// <summary>CU-01 Configurar partida. Cada opción parte del valor predeterminado; Esc cancela.</summary>
    private ConfiguracionPartida LeerConfiguracion()
    {
        var resumen = new List<string>();
        var alias = LeerAlias(resumen);
        resumen.Add($"Alias: {alias}");

        var tipo = Elegir("Tipo de dominó", [TipoDomino.Doble6, TipoDomino.Doble9], TipoDomino.Doble6, resumen);
        resumen.Add($"Tipo de dominó: {tipo}");
        var cantidadCpu = Elegir("Número de CPU", [1, 2, 3], 1, resumen);
        resumen.Add($"Número de CPU: {cantidadCpu}");

        var niveles = new List<NivelDificultad>();
        for (var i = 1; i <= cantidadCpu; i++)
        {
            var nivel = Elegir($"Nivel de la CPU {i}", Enum.GetValues<NivelDificultad>(), NivelDificultad.Intermedio, resumen);
            niveles.Add(nivel);
            resumen.Add($"CPU {i}: {nivel}");
        }
        var limite = Elegir("Límite de tiempo por turno", Enum.GetValues<LimiteTiempo>(), LimiteTiempo.SinLimite, resumen);

        return new ConfiguracionPartida(alias, tipo, niveles, limite);
    }

    private string LeerAlias(IReadOnlyList<string> resumen)
    {
        var mensaje = "";
        while (true)
        {
            pantalla.DibujarMenu("Configurar partida", [], 0, resumen, mensaje);
            var alias = pantalla.LeerTexto("Alias: ");
            try
            {
                ConfiguracionPartida.Predeterminada(alias).Validar();
                return alias;
            }
            catch (ArgumentException ex)
            {
                mensaje = ex.Message;
            }
        }
    }

    /// <summary>Ciclo de turnos del humano hasta que la partida termina o el jugador sale.</summary>
    private VistaPartida Jugar(VistaPartida vista)
    {
        var seleccion = PrimeraJugable(vista);
        var mensaje = "";
        while (vista.Estado != EstadoPartida.Terminada)
        {
            pantalla.DibujarTablero(vista, seleccion, mensaje);
            var tecla = EsperarTecla(ref vista, seleccion, ref mensaje);
            if (tecla is null)
            {
                seleccion = PrimeraJugable(vista);
                continue;
            }

            mensaje = "";
            var anterior = vista;
            switch (tecla.Value.Key)
            {
                case ConsoleKey.LeftArrow:
                    seleccion = MoverSeleccion(vista, seleccion, -1);
                    continue;
                case ConsoleKey.RightArrow:
                    seleccion = MoverSeleccion(vista, seleccion, +1);
                    continue;
                case ConsoleKey.Enter or ConsoleKey.Spacebar:
                    (vista, mensaje) = JugarSeleccion(vista, seleccion);
                    break;
                case ConsoleKey.R:
                    (vista, mensaje) = Intentar(vista, controlador.Robar);
                    break;
                case ConsoleKey.P:
                    (vista, mensaje) = Intentar(vista, controlador.Pasar);
                    break;
                case ConsoleKey.D:
                    (vista, mensaje) = Intentar(vista, controlador.DeshacerUltimaJugada, "Jugada deshecha.");
                    break;
                case ConsoleKey.G:
                    mensaje = Guardar(vista, seleccion);
                    continue;
                case ConsoleKey.S or ConsoleKey.Escape:
                    return vista;
                default:
                    continue;
            }
            if (!ReferenceEquals(vista, anterior))
                seleccion = PrimeraJugable(vista);
        }

        pantalla.DibujarFinal(vista);
        Console.ReadKey(intercept: true);
        return vista;
    }

    /// <summary>
    /// Espera una tecla. Si el turno tiene límite de tiempo, actualiza la cuenta regresiva y, si se
    /// agota, devuelve null con la vista ya actualizada tras la pérdida del turno (RN-04).
    /// </summary>
    private ConsoleKeyInfo? EsperarTecla(ref VistaPartida vista, int seleccion, ref string mensaje)
    {
        while (vista.SegundosRestantes is not null && !Console.KeyAvailable)
        {
            Thread.Sleep(150);
            var actual = controlador.VerificarTiempo();
            if (UltimoOrden(actual) != UltimoOrden(vista))
            {
                vista = actual;
                mensaje = "Se agotó tu tiempo: perdiste el turno.";
                return null;
            }
            if (actual.SegundosRestantes != vista.SegundosRestantes)
            {
                vista = actual;
                pantalla.DibujarTablero(vista, seleccion, mensaje);
            }
        }
        return Console.ReadKey(intercept: true);
    }

    private (VistaPartida, string) JugarSeleccion(VistaPartida vista, int seleccion)
    {
        if (seleccion < 0 || seleccion >= vista.ManoPropia.Count)
            return (vista, vista.PuedeRobar ? "No tienes ficha para lanzar: roba con R."
                : vista.PuedePasar ? "No tienes ficha para lanzar ni fichas para robar: pasa con P." : "");

        var ficha = vista.ManoPropia[seleccion];
        var opciones = vista.Opciones.Where(o => o.Ficha == ficha).ToList();
        if (opciones.Count == 0)
            return (vista, $"La ficha {ficha} no se puede jugar ahora.");

        var opcion = opciones.Count == 1 ? opciones[0] : ElegirExtremo(vista, seleccion, opciones);
        return opcion is null ? (vista, "") : Intentar(vista, () => controlador.JugarFicha(opcion.Ficha, opcion.Extremo));
    }

    private OpcionJugada? ElegirExtremo(VistaPartida vista, int seleccion, IReadOnlyList<OpcionJugada> opciones)
    {
        pantalla.DibujarTablero(vista, seleccion, "¿En qué extremo?  ← izquierdo · → derecho · Esc cancelar");
        while (true)
        {
            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.LeftArrow:
                    return opciones.First(o => o.Extremo == Extremo.Izquierdo);
                case ConsoleKey.RightArrow:
                    return opciones.First(o => o.Extremo == Extremo.Derecho);
                case ConsoleKey.Escape:
                    return null;
            }
        }
    }

    private string Guardar(VistaPartida vista, int seleccion)
    {
        pantalla.DibujarTablero(vista, seleccion, "");
        var nombre = pantalla.LeerTexto("Nombre de la partida (Enter vacío cancela): ");
        if (nombre.Length == 0)
            return "Guardado cancelado.";
        try
        {
            controlador.GuardarPartida(nombre);
            return $"Partida guardada como «{nombre}».";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException)
        {
            return ex.Message;
        }
    }

    private static (VistaPartida, string) Intentar(VistaPartida vista, Func<VistaPartida> accion, string exito = "")
    {
        try
        {
            return (accion(), exito);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return (vista, ex.Message);
        }
    }

    private T Elegir<T>(string titulo, IReadOnlyList<T> opciones, T predeterminado, IReadOnlyList<string> resumen, string mensaje = "")
    {
        var seleccion = Math.Max(0, opciones.ToList().IndexOf(predeterminado));
        var textos = opciones.Select(o => o?.ToString() ?? "").ToList();
        while (true)
        {
            pantalla.DibujarMenu(titulo, textos, seleccion, resumen, mensaje);
            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.UpArrow:
                    seleccion = (seleccion - 1 + opciones.Count) % opciones.Count;
                    break;
                case ConsoleKey.DownArrow:
                    seleccion = (seleccion + 1) % opciones.Count;
                    break;
                case ConsoleKey.Enter:
                    return opciones[seleccion];
                case ConsoleKey.Escape:
                    throw new OperationCanceledException();
            }
        }
    }

    /// <summary>Posiciones de la mano que tienen ficha jugable; la selección solo recorre estas.</summary>
    private static List<int> Jugables(VistaPartida vista)
    {
        var fichas = vista.Opciones.Select(o => o.Ficha).ToHashSet();
        return Enumerable.Range(0, vista.ManoPropia.Count).Where(i => fichas.Contains(vista.ManoPropia[i])).ToList();
    }

    /// <summary>Primera ficha jugable de la mano, o -1 si no hay ninguna.</summary>
    private static int PrimeraJugable(VistaPartida vista) => Jugables(vista).DefaultIfEmpty(-1).First();

    private static int MoverSeleccion(VistaPartida vista, int seleccion, int paso)
    {
        var jugables = Jugables(vista);
        if (jugables.Count == 0)
            return -1;
        var actual = Math.Max(0, jugables.IndexOf(seleccion));
        return jugables[(actual + paso + jugables.Count) % jugables.Count];
    }

    private static int UltimoOrden(VistaPartida vista) =>
        vista.UltimasJugadas.Count > 0 ? vista.UltimasJugadas[^1].Orden : 0;
}
