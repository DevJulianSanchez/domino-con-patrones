namespace Domino.Dominio;

/// <summary>
/// Pieza del dominó. Se normaliza con ValorA &lt;= ValorB para que dos fichas iguales
/// sean iguales sin importar su orientación.
/// </summary>
public sealed record Ficha
{
    public Ficha(int valorA, int valorB)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valorA);
        ArgumentOutOfRangeException.ThrowIfNegative(valorB);
        ValorA = Math.Min(valorA, valorB);
        ValorB = Math.Max(valorA, valorB);
    }

    public int ValorA { get; }

    public int ValorB { get; }

    public int Puntos => ValorA + ValorB;

    public bool EsDoble => ValorA == ValorB;

    public override string ToString() => $"[{ValorA}|{ValorB}]";
}
