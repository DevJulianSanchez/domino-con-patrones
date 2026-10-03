namespace Domino.Dominio;

public abstract class Jugador
{
    protected Jugador(string alias, int posicion)
    {
        Alias = alias;
        Posicion = posicion;
    }

    public string Alias { get; }

    public int Posicion { get; }

    public Mano Mano { get; } = new();

    public abstract bool EsAutomatico { get; }

    public abstract bool AplicaLimiteTiempo { get; }
}
