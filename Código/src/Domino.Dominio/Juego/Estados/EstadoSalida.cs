namespace Domino.Dominio;

/// <summary>
/// Apertura de la mano (RN-05): solo se admite la ficha de salida, en el turno de quien la tiene.
/// El límite de tiempo no corre, porque la jugada es obligada.
/// </summary>
internal sealed class EstadoSalida : EstadoJuego
{
    public override EstadoPartida Valor => EstadoPartida.Salida;

    public override bool EnJuego => true;

    public override IReadOnlyList<OpcionJugada> OpcionesDe(Partida partida, Jugador jugador) =>
        partida.FichaSalida is { } ficha ? [new OpcionJugada(ficha, Extremo.Izquierdo)] : [];

    public override void Procesar(Partida partida, Jugada jugada)
    {
        if (jugada is not JugadaColocarFicha colocar || colocar.Ficha != partida.FichaSalida || !partida.EsValida(jugada))
            throw new InvalidOperationException($"La mano debe abrirse con la ficha {partida.FichaSalida}.");

        partida.CambiarEstado(new EstadoEnCurso());
        partida.Aplicar(jugada);
    }
}
