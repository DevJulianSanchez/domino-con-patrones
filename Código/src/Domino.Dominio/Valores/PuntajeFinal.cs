namespace Domino.Dominio;

/// <summary>Puntos y fichas que revela un jugador al terminar la partida.</summary>
public sealed record PuntajeFinal(string Alias, int Posicion, int Puntos, IReadOnlyList<Ficha> Fichas)
{
    public int CantidadFichas => Fichas.Count;

    public static PuntajeFinal De(Jugador jugador) =>
        new(jugador.Alias, jugador.Posicion, jugador.Mano.SumaPuntos, jugador.Mano.Fichas.ToList());
}
