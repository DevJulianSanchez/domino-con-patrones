namespace Domino.Dominio;

/// <summary>Pasa el turno: solo se admite sin ficha para lanzar y sin fichas para robar (RN-07).</summary>
public sealed class JugadaPasar : Jugada
{
    public JugadaPasar(int orden, int posicionJugador) : base(orden, posicionJugador)
    {
    }

    public override TipoJugada Tipo => TipoJugada.Pasar;

    internal override bool TerminaTurno => true;

    internal override bool EsValida(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar) =>
        fichasParaRobar.EstaVacia && !mesa.AdmiteAlgunaDe(mano.Fichas);

    internal override void Ejecutar(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
    }

    internal override void Deshacer(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
    }
}
