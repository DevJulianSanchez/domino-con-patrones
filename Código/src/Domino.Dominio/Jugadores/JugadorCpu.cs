namespace Domino.Dominio;

/// <summary>Jugador automático con el nivel de dificultad elegido en la configuración.</summary>
public sealed class JugadorCpu(string alias, int posicion, NivelDificultad nivel) : Jugador(alias, posicion)
{
    public override bool EsAutomatico => true;

    public NivelDificultad Nivel { get; } = nivel;
}
