namespace Domino.Dominio;

/// <summary>
/// Lo necesario para reconstruir una partida: con la misma configuración y semilla se
/// repite el reparto, y luego se vuelven a aplicar las jugadas en orden (RN-11).
/// </summary>
public sealed record PartidaGuardada(ConfiguracionPartida Configuracion, int Semilla, IReadOnlyList<Jugada> Jugadas);
