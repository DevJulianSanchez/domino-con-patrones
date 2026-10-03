namespace Domino.Dominio;

/// <summary>Fichas de un jugador.</summary>
public sealed class Mano
{
    private readonly List<Ficha> fichas = [];

    public IReadOnlyList<Ficha> Fichas => fichas;

    public int Cantidad => fichas.Count;

    public bool EstaVacia => fichas.Count == 0;

    public int SumaPuntos => fichas.Sum(f => f.Puntos);

    public void Agregar(Ficha ficha) => fichas.Add(ficha);

    public void Quitar(Ficha ficha)
    {
        if (!fichas.Remove(ficha))
            throw new InvalidOperationException($"La mano no contiene la ficha {ficha}.");
    }

    public bool Contiene(Ficha ficha) => fichas.Contains(ficha);

    public Ficha? MayorDoble() => fichas.Where(f => f.EsDoble).MaxBy(f => f.ValorA);

    /// <summary>Ficha de mayor puntaje; en empate, la de número individual más alto (RN-05).</summary>
    public Ficha? FichaDeMayorPuntaje() =>
        fichas.OrderByDescending(f => f.Puntos).ThenByDescending(f => f.ValorMayor).FirstOrDefault();
}
