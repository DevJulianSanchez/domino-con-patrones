namespace Domino.Dominio;

/// <summary>Genera, baraja y reparte el juego de fichas con una semilla reproducible (RN-01, RN-03).</summary>
public sealed class Repartidor
{
    private readonly Random aleatorio;

    public Repartidor(int semilla)
    {
        aleatorio = new Random(semilla);
    }

    public List<Ficha> GenerarJuego(DefinicionTipoDomino definicion)
    {
        var fichas = new List<Ficha>(definicion.TotalFichas);
        for (var a = 0; a <= definicion.ValorMaximo; a++)
            for (var b = a; b <= definicion.ValorMaximo; b++)
                fichas.Add(new Ficha(a, b));
        return fichas;
    }

    public FichasParaRobar Repartir(IReadOnlyList<Jugador> jugadores, DefinicionTipoDomino definicion)
    {
        if (jugadores.Count * definicion.FichasPorJugador > definicion.TotalFichas)
            throw new InvalidOperationException("No hay fichas suficientes para todos los jugadores.");

        var fichas = GenerarJuego(definicion);
        Barajar(fichas);

        var siguiente = 0;
        foreach (var jugador in jugadores)
            for (var i = 0; i < definicion.FichasPorJugador; i++)
                jugador.Mano.Agregar(fichas[siguiente++]);

        return new FichasParaRobar(fichas.Skip(siguiente));
    }

    /// <summary>Fisher-Yates con el generador de la semilla.</summary>
    private void Barajar(List<Ficha> fichas)
    {
        for (var i = fichas.Count - 1; i > 0; i--)
        {
            var j = aleatorio.Next(i + 1);
            (fichas[i], fichas[j]) = (fichas[j], fichas[i]);
        }
    }
}
