namespace Domino.Dominio;

/// <summary>Jugador cuyas decisiones llegan desde la interfaz.</summary>
public sealed class JugadorHumano(string alias, int posicion) : Jugador(alias, posicion)
{
    public override bool EsAutomatico => false;
}
