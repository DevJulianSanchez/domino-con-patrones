namespace Domino.Dominio;

/// <summary>Coloca una ficha de la mano en un extremo de la mesa (RN-06).</summary>
public sealed class JugadaColocarFicha : Jugada
{
    public JugadaColocarFicha(int orden, int posicionJugador, Ficha ficha, Extremo extremo) : base(orden, posicionJugador)
    {
        Ficha = ficha;
        Extremo = extremo;
    }

    public Ficha Ficha { get; }

    public Extremo Extremo { get; }

    public override TipoJugada Tipo => TipoJugada.ColocarFicha;

    internal override bool TerminaTurno => true;

    internal override bool EsValida(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar) =>
        mano.Contiene(Ficha) && mesa.PuedeColocar(Ficha, Extremo);

    internal override void Ejecutar(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar)
    {
        mano.Quitar(Ficha);
        mesa.Colocar(Ficha, Extremo);
    }

    internal override void Deshacer(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar) =>
        mano.Agregar(mesa.RetirarDe(Extremo));
}
