namespace Domino.Dominio;

/// <summary>Fichas que quedan después del reparto (RN-03, RN-07).</summary>
public sealed class FichasParaRobar
{
    private readonly Stack<Ficha> fichas;

    public FichasParaRobar(IEnumerable<Ficha> fichas)
    {
        this.fichas = new Stack<Ficha>(fichas);
    }

    public int Cantidad => fichas.Count;

    public bool EstaVacia => fichas.Count == 0;

    public Ficha Robar() =>
        fichas.Count > 0 ? fichas.Pop() : throw new InvalidOperationException("No quedan fichas para robar.");

    public void Devolver(Ficha ficha) => fichas.Push(ficha);
}
