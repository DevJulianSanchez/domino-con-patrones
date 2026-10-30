namespace Domino.Dominio;

/// <summary>Partida configurada y sin repartir: solo admite iniciar.</summary>
internal sealed class EstadoPreparada : EstadoJuego
{
    public override EstadoPartida Valor => EstadoPartida.Preparada;

    public override void Iniciar(Partida partida)
    {
        partida.Repartir();
        partida.CambiarEstado(new EstadoSalida());
    }
}
