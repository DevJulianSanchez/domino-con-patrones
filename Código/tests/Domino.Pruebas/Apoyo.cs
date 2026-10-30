using Domino.Dominio;
using Domino.Persistencia;

namespace Domino.Pruebas;

/// <summary>Reloj controlado por la prueba.</summary>
internal sealed class RelojFalso : IReloj
{
    public DateTime Ahora { get; private set; } = new(2026, 1, 1);

    public void Avanzar(int segundos) => Ahora = Ahora.AddSeconds(segundos);
}

internal static class Apoyo
{
    public static ConfiguracionPartida Configuracion(
        TipoDomino tipo = TipoDomino.Doble9, int cpus = 3, LimiteTiempo limite = LimiteTiempo.SinLimite) =>
        new("Prueba", tipo,
            Enumerable.Range(0, cpus).Select(i => (NivelDificultad)(i % 3)).ToList(),
            limite);

    public static ControladorJuego Controlador(int semilla, IReloj? reloj = null) =>
        new(reloj ?? new RelojFalso(), new RepositorioPartidasJson(CarpetaTemporal()), new Random(semilla));

    public static string CarpetaTemporal() =>
        Path.Combine(Path.GetTempPath(), "DominoPruebas", Guid.NewGuid().ToString("N"));

    /// <summary>Acción del humano simulado: la primera opción, o robar, o pasar.</summary>
    public static VistaPartida JugarTurno(ControladorJuego controlador, VistaPartida vista)
    {
        if (vista.Opciones.Count > 0)
            return controlador.JugarFicha(vista.Opciones[0].Ficha, vista.Opciones[0].Extremo);
        return vista.PuedeRobar ? controlador.Robar() : controlador.Pasar();
    }

    /// <summary>Avanza la partida hasta que se cumpla la condición; null si terminó antes.</summary>
    public static VistaPartida? AvanzarHasta(ControladorJuego controlador, VistaPartida vista, Func<VistaPartida, bool> condicion)
    {
        while (vista.Estado != EstadoPartida.Terminada)
        {
            if (condicion(vista))
                return vista;
            vista = JugarTurno(controlador, vista);
        }
        return null;
    }

    public static int TotalFichas(VistaPartida vista) =>
        vista.FichasPorJugador.Sum() + vista.Mesa.Count + vista.FichasParaRobar;

    public static IEnumerable<Ficha> Ordenadas(IEnumerable<Ficha> fichas) =>
        fichas.OrderBy(f => f.ValorA).ThenBy(f => f.ValorB);

    /// <summary>Compara lo observable de dos vistas; la mano se compara sin importar el orden.</summary>
    public static void AssertMismoEstado(VistaPartida esperada, VistaPartida actual)
    {
        Assert.Equal(esperada.Estado, actual.Estado);
        Assert.Equal(esperada.PosicionEnTurno, actual.PosicionEnTurno);
        Assert.Equal(esperada.FichasPorJugador, actual.FichasPorJugador);
        Assert.Equal(esperada.FichasParaRobar, actual.FichasParaRobar);
        Assert.Equal(esperada.Mesa, actual.Mesa);
        Assert.Equal(Ordenadas(esperada.ManoPropia), Ordenadas(actual.ManoPropia));
    }

    public static VistaPartida Vista(IReadOnlyList<Ficha> mano, int? izquierdo, int? derecho) => new()
    {
        Estado = EstadoPartida.EnCurso,
        Jugadores = ["CPU", "Otro"],
        FichasPorJugador = [mano.Count, 5],
        PosicionPropia = 0,
        PosicionEnTurno = 0,
        EsTurnoPropio = true,
        ManoPropia = mano,
        Mesa = [],
        ExtremoIzquierdo = izquierdo,
        ExtremoDerecho = derecho,
        FichasParaRobar = 0,
        Opciones = [],
        PuedeRobar = false,
        PuedePasar = false,
        PuedeDeshacer = false,
        SegundosRestantes = null,
        UltimasJugadas = [],
        Resultado = null
    };
}
