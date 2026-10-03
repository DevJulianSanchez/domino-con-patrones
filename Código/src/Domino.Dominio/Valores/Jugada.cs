namespace Domino.Dominio;

/// <summary>Acción registrada en el historial. El jugador se identifica por su posición.</summary>
public sealed record Jugada(int Orden, TipoJugada Tipo, int PosicionJugador, Ficha? Ficha, Extremo? Extremo);
