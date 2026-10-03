namespace Domino.Dominio;

/// <summary>Fichas de un jugador.</summary>
public sealed class Mano
{
    private readonly List<Ficha> fichas = [];

    public IReadOnlyList<Ficha> Fichas => fichas;

    public int Cantidad => fichas.Count;

    public void Agregar(Ficha ficha) => fichas.Add(ficha);
}
