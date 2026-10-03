namespace Domino.Dominio;

/// <summary>Secuencia ordenada de jugadas de la partida (RN-10, RN-11).</summary>
public sealed class HistorialJugadas
{
    private readonly List<Jugada> jugadas = [];

    public int Cantidad => jugadas.Count;

    public IReadOnlyList<Jugada> Todas => jugadas;

    public void Registrar(Jugada jugada) => jugadas.Add(jugada);

    public bool HayJugadaDe(int posicion) => jugadas.Exists(j => j.PosicionJugador == posicion);

    /// <summary>Retira y devuelve, de la más reciente a la más antigua, las jugadas desde la última del jugador.</summary>
    public IReadOnlyList<Jugada> ExtraerHastaUltimaDe(int posicion)
    {
        var indice = jugadas.FindLastIndex(j => j.PosicionJugador == posicion);
        if (indice < 0)
            return [];

        var extraidas = jugadas.Skip(indice).Reverse().ToList();
        jugadas.RemoveRange(indice, jugadas.Count - indice);
        return extraidas;
    }

    public IReadOnlyList<Jugada> Ultimas(int cantidad) => jugadas.TakeLast(cantidad).ToList();
}
