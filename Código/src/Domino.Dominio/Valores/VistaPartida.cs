namespace Domino.Dominio;

/// <summary>
/// Información visible para un jugador. Solo contiene tipos de valor, de modo que quien
/// la recibe no puede modificar el dominio a través de ella.
/// </summary>
public sealed record VistaPartida
{
    public required EstadoPartida Estado { get; init; }

    /// <summary>Alias de los jugadores, indexados por posición.</summary>
    public required IReadOnlyList<string> Jugadores { get; init; }

    /// <summary>Cantidad de fichas de cada jugador, indexada por posición.</summary>
    public required IReadOnlyList<int> FichasPorJugador { get; init; }

    public required int PosicionPropia { get; init; }

    public required int PosicionEnTurno { get; init; }

    public required bool EsTurnoPropio { get; init; }

    public required IReadOnlyList<Ficha> ManoPropia { get; init; }

    public required IReadOnlyList<FichaColocada> Mesa { get; init; }

    public required int? ExtremoIzquierdo { get; init; }

    public required int? ExtremoDerecho { get; init; }

    public required int FichasParaRobar { get; init; }

    public required IReadOnlyList<OpcionJugada> Opciones { get; init; }

    public required bool PuedeRobar { get; init; }

    public required bool PuedePasar { get; init; }

    public required bool PuedeDeshacer { get; init; }

    public required int? SegundosRestantes { get; init; }

    public required IReadOnlyList<Jugada> UltimasJugadas { get; init; }

    public required Resultado? Resultado { get; init; }
}
