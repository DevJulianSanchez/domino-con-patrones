namespace Domino.Dominio;

/// <summary>Forma en que terminó la partida, puntajes revelados y ganador (RN-08, RN-09).</summary>
public sealed record Resultado(FormaDeCierre Forma, PuntajeFinal? Ganador, IReadOnlyList<PuntajeFinal> Puntajes)
{
    public bool EsEmpate => Ganador is null;

    public static Resultado PorDomino(Jugador ganador, IReadOnlyList<Jugador> jugadores)
    {
        var puntajes = jugadores.Select(PuntajeFinal.De).ToList();
        return new Resultado(FormaDeCierre.Domino, puntajes.Single(p => p.Posicion == ganador.Posicion), puntajes);
    }

    /// <summary>Gana la menor suma de puntos; en empate, quien tenga menos fichas; si persiste, no hay ganador.</summary>
    public static Resultado PorJuegoCerrado(IReadOnlyList<Jugador> jugadores)
    {
        var puntajes = jugadores.Select(PuntajeFinal.De).ToList();
        var ordenados = puntajes.OrderBy(p => p.Puntos).ThenBy(p => p.CantidadFichas).ToList();
        var mejor = ordenados[0];
        var empatado = ordenados.Skip(1).Any(p => p.Puntos == mejor.Puntos && p.CantidadFichas == mejor.CantidadFichas);
        return new Resultado(FormaDeCierre.JuegoCerrado, empatado ? null : mejor, puntajes);
    }
}
