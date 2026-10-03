namespace Domino.Dominio;

/// <summary>Jugador automático: decide su jugada a partir de las opciones válidas y de su propia vista.</summary>
public abstract class JugadorCpu : Jugador
{
    protected JugadorCpu(string alias, int posicion) : base(alias, posicion)
    {
    }

    public override bool EsAutomatico => true;

    public override bool AplicaLimiteTiempo => false;

    public abstract NivelDificultad Nivel { get; }

    public abstract OpcionJugada DecidirJugada(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista);
}
