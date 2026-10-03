namespace Domino.Dominio;

/// <summary>Jugador cuyas decisiones llegan desde la interfaz; le aplica el límite de tiempo (RN-04).</summary>
public sealed class JugadorHumano(string alias, int posicion) : Jugador(alias, posicion)
{
    public override bool EsAutomatico => false;

    public override bool AplicaLimiteTiempo => true;
}
