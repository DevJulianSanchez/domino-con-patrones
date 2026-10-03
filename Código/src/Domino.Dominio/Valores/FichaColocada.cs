namespace Domino.Dominio;

/// <summary>Ficha orientada en la mesa, con el valor que queda hacia cada lado.</summary>
public sealed record FichaColocada(Ficha Ficha, int ValorIzquierdo, int ValorDerecho);
