namespace Domino.Dominio;

/// <summary>Cadena de fichas colocadas con sus dos extremos abiertos (RN-06).</summary>
public sealed class Mesa
{
    private readonly LinkedList<FichaColocada> colocadas = new();

    public bool EstaVacia => colocadas.Count == 0;

    public IReadOnlyList<FichaColocada> Colocadas => colocadas.ToList();

    public int ValorExtremo(Extremo extremo)
    {
        if (EstaVacia)
            throw new InvalidOperationException("La mesa está vacía.");
        return extremo == Extremo.Izquierdo ? colocadas.First!.Value.ValorIzquierdo : colocadas.Last!.Value.ValorDerecho;
    }

    public bool PuedeColocar(Ficha ficha, Extremo extremo) => EstaVacia || ficha.Contiene(ValorExtremo(extremo));

    public void Colocar(Ficha ficha, Extremo extremo)
    {
        if (!PuedeColocar(ficha, extremo))
            throw new InvalidOperationException($"La ficha {ficha} no puede colocarse en el extremo {extremo}.");

        if (EstaVacia)
        {
            colocadas.AddFirst(new FichaColocada(ficha, ficha.ValorA, ficha.ValorB));
            return;
        }

        var valor = ValorExtremo(extremo);
        if (extremo == Extremo.Izquierdo)
            colocadas.AddFirst(new FichaColocada(ficha, ficha.OtroValor(valor), valor));
        else
            colocadas.AddLast(new FichaColocada(ficha, valor, ficha.OtroValor(valor)));
    }

    public Ficha RetirarDe(Extremo extremo)
    {
        var nodo = (extremo == Extremo.Izquierdo ? colocadas.First : colocadas.Last)
                   ?? throw new InvalidOperationException("La mesa está vacía.");
        colocadas.Remove(nodo);
        return nodo.Value.Ficha;
    }
}
