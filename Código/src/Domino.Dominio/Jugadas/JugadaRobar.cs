namespace Domino.Dominio;

/// <summary>
/// Roba una ficha cuando el jugador no tiene ficha para lanzar (RN-07). El turno no termina:
/// el jugador sigue robando hasta tener una ficha jugable o agotar las fichas para robar.
/// </summary>
public sealed class JugadaRobar : Jugada
{
    private Ficha? fichaRobada;

    public JugadaRobar(int orden, int posicionJugador) : base(orden, posicionJugador)
    {
    }

    public override TipoJugada Tipo => TipoJugada.Robar;

    internal override bool TerminaTurno => false;

    internal override bool EsValida(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar) =>
        !fichasParaRobar.EstaVacia && !mesa.AdmiteAlgunaDe(mano.Fichas);

    internal override void Ejecutar(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
        fichaRobada = fichasParaRobar.Robar();
        mano.Agregar(fichaRobada);
    }

    internal override void Deshacer(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
        var ficha = fichaRobada ?? throw new InvalidOperationException("La jugada de robo no se ha ejecutado.");
        mano.Quitar(ficha);
        fichasParaRobar.Devolver(ficha);
        fichaRobada = null;
    }
}
