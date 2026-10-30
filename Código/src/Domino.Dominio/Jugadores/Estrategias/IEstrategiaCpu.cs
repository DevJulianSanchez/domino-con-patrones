namespace Domino.Dominio;

/// <summary>
/// Strategy: criterio con el que un jugador automático elige entre las opciones válidas.
/// Cada nivel de dificultad es una estrategia; agregar un nivel no modifica JugadorCpu ni Partida.
/// </summary>
public interface IEstrategiaCpu
{
    NivelDificultad Nivel { get; }

    OpcionJugada Elegir(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista);
}
