namespace Domino.Dominio;

/// <summary>
/// Pérdida del turno por tiempo agotado (RN-04). Cuándo se agota el tiempo lo decide la partida;
/// la jugada solo registra el hecho para que el historial y la partida guardada lo conserven.
/// </summary>
public sealed class JugadaTiempoAgotado : Jugada
{
    public JugadaTiempoAgotado(int orden, int posicionJugador) : base(orden, posicionJugador)
    {
    }

    public override TipoJugada Tipo => TipoJugada.TiempoAgotado;

    internal override bool TerminaTurno => true;

    internal override bool EsValida(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar) => true;

    internal override void Ejecutar(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
    }

    internal override void Deshacer(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
    }
}
