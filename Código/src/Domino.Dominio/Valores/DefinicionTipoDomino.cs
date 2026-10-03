namespace Domino.Dominio;

/// <summary>Características de cada tipo de dominó (RN-01, RN-03).</summary>
public sealed record DefinicionTipoDomino(TipoDomino Tipo, int ValorMaximo, int FichasPorJugador)
{
    public int TotalFichas => (ValorMaximo + 1) * (ValorMaximo + 2) / 2;

    public static DefinicionTipoDomino De(TipoDomino tipo) => tipo switch
    {
        TipoDomino.Doble6 => new DefinicionTipoDomino(tipo, ValorMaximo: 6, FichasPorJugador: 7),
        TipoDomino.Doble9 => new DefinicionTipoDomino(tipo, ValorMaximo: 9, FichasPorJugador: 10),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de dominó no soportado.")
    };
}
