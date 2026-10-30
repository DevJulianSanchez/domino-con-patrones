namespace Domino.Dominio;

/// <summary>
/// Jugador automático. Delega la decisión de jugada en su estrategia (Strategy), que define
/// su nivel de dificultad.
/// </summary>
public sealed class JugadorCpu : Jugador
{
    private readonly IEstrategiaCpu estrategia;

    public JugadorCpu(string alias, int posicion, IEstrategiaCpu estrategia) : base(alias, posicion)
    {
        this.estrategia = estrategia;
    }

    public override bool EsAutomatico => true;

    public override bool AplicaLimiteTiempo => false;

    public NivelDificultad Nivel => estrategia.Nivel;

    public OpcionJugada DecidirJugada(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        estrategia.Elegir(opciones, vista);
}
