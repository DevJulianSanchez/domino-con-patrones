namespace Domino.Dominio;

/// <summary>Jugada válida disponible: una ficha de la mano y el extremo donde puede colocarse.</summary>
public sealed record OpcionJugada(Ficha Ficha, Extremo Extremo);
