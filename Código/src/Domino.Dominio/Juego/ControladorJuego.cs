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

    public VistaPartida IniciarPartida(ConfiguracionPartida configuracion)
    {
        partida = new Partida(configuracion, aleatorio.Next(), reloj);
        partida.Iniciar();
        return ObtenerVista();
    }

    public VistaPartida ObtenerVista()
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        return actual.ObtenerVistaPara(actual.Humano);
    }

    public VistaPartida JugarFicha(Ficha ficha, Extremo extremo)
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        if (!actual.VerificarTiempo())
            actual.JugarFicha(ficha, extremo);
        actual.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public VistaPartida Robar()
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        if (!actual.VerificarTiempo())
            actual.Robar();
        actual.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public VistaPartida Pasar()
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        if (!actual.VerificarTiempo())
            actual.Pasar();
        actual.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public VistaPartida DeshacerUltimaJugada()
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        actual.DeshacerUltimaJugadaHumano();
        return ObtenerVista();
    }

    public VistaPartida VerificarTiempo()
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        if (actual.VerificarTiempo())
            actual.ResolverTurnosCpu();
        return ObtenerVista();
    }

    public void GuardarPartida(string nombre)
    {
        var actual = partida ?? throw new InvalidOperationException("No hay una partida en curso.");
        repositorio.Guardar(nombre, actual.CrearGuardado());
    }

    public VistaPartida CargarPartida(string nombre)
    {
        partida = Partida.Restaurar(repositorio.Cargar(nombre), reloj);
        return ObtenerVista();
    }

    public IReadOnlyList<string> ListarPartidasGuardadas() => repositorio.ListarNombres();
}
