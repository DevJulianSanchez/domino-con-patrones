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

    public int ValorMayor => ValorB;

    public bool Contiene(int valor) => ValorA == valor || ValorB == valor;

    public int OtroValor(int valor)
    {
        if (valor == ValorA) return ValorB;
        if (valor == ValorB) return ValorA;
        throw new ArgumentException($"La ficha {this} no contiene el valor {valor}.", nameof(valor));
    }

    public override string ToString() => $"[{ValorA}|{ValorB}]";
}
