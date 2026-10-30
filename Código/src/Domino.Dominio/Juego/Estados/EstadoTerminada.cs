namespace Domino.Dominio;

/// <summary>Partida terminada por dominó o por juego cerrado: no admite más acciones.</summary>
internal sealed class EstadoTerminada : EstadoJuego
{
    public override EstadoPartida Valor => EstadoPartida.Terminada;
}
