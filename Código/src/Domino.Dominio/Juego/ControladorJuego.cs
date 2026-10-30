namespace Domino.Dominio;

/// <summary>
/// Punto de entrada de las operaciones del sistema. Delega en la partida, resuelve después los
/// turnos de las CPU y devuelve siempre la vista del jugador humano.
/// </summary>
public sealed class ControladorJuego
{
    private readonly IReloj reloj;
    private readonly IRepositorioPartidas repositorio;
    private readonly Random aleatorio;
    private Partida? partida;

    public ControladorJuego(IReloj reloj, IRepositorioPartidas repositorio, Random aleatorio)
    {
        this.reloj = reloj;
        this.repositorio = repositorio;
        this.aleatorio = aleatorio;
    }

    /// <summary>Crea e inicia la partida; si la salida le corresponde a una CPU, la juega.</summary>
    public VistaPartida IniciarPartida(ConfiguracionPartida configuracion)
    {
        partida = new Partida(configuracion, aleatorio.Next(), reloj);
        partida.Iniciar();
        partida.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public VistaPartida ObtenerVista()
    {
        var actual = PartidaActual();
        return actual.ObtenerVistaPara(actual.Humano);
    }

    public VistaPartida JugarFicha(Ficha ficha, Extremo extremo) => AccionDelHumano(p => p.JugarFicha(ficha, extremo));

    public VistaPartida Robar() => AccionDelHumano(p => p.Robar());

    public VistaPartida Pasar() => AccionDelHumano(p => p.Pasar());

    public VistaPartida DeshacerUltimaJugada()
    {
        PartidaActual().DeshacerUltimaJugadaHumano();
        return ObtenerVista();
    }

    /// <summary>Permite a la interfaz comprobar el tiempo del turno sin que el jugador actúe.</summary>
    public VistaPartida VerificarTiempo()
    {
        var actual = PartidaActual();
        if (actual.VerificarTiempo())
            actual.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public void GuardarPartida(string nombre) => repositorio.Guardar(nombre, PartidaActual().CrearGuardado());

    public VistaPartida CargarPartida(string nombre)
    {
        partida = Partida.Restaurar(repositorio.Cargar(nombre), reloj);
        partida.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public IReadOnlyList<string> ListarPartidasGuardadas() => repositorio.ListarNombres();

    private Partida PartidaActual() => partida ?? throw new InvalidOperationException("No hay una partida en curso.");

    /// <summary>
    /// Si el tiempo del humano se agotó, la acción se descarta y pierde el turno (RN-04); si no,
    /// se ejecuta. En ambos casos se resuelven después los turnos de las CPU.
    /// </summary>
    private VistaPartida AccionDelHumano(Action<Partida> accion)
    {
        var actual = PartidaActual();
        if (!actual.VerificarTiempo())
        {
            if (actual.JugadorEnTurno != actual.Humano)
                throw new InvalidOperationException("No es el turno del jugador.");
            accion(actual);
        }
        actual.ResolverTurnosCpu();
        return ObtenerVista();
    }
}
