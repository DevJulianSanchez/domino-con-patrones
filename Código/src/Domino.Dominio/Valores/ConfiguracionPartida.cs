namespace Domino.Dominio;

/// <summary>Datos elegidos por el jugador en CU-01 Configurar partida.</summary>
public sealed record ConfiguracionPartida(
    string Alias,
    TipoDomino Tipo,
    IReadOnlyList<NivelDificultad> NivelesCpu,
    LimiteTiempo Limite)
{
    public const int LongitudMaximaAlias = 15;

    public int? SegundosPorTurno => Limite switch
    {
        LimiteTiempo.SinLimite => null,
        LimiteTiempo.Segundos10 => 10,
        LimiteTiempo.Segundos20 => 20,
        LimiteTiempo.Segundos30 => 30,
        _ => throw new InvalidOperationException($"Límite de tiempo no soportado: {Limite}.")
    };

    /// <summary>Verifica RN-02 y el alias de CU-01 (curso alterno 2a).</summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Alias))
            throw new ArgumentException("El alias no puede estar vacío.");
        if (Alias.Trim().Length > LongitudMaximaAlias)
            throw new ArgumentException($"El alias no puede superar {LongitudMaximaAlias} caracteres.");
        if (NivelesCpu.Count is < 1 or > 3)
            throw new ArgumentException("La partida admite de una a tres CPU.");
    }

    /// <summary>Configuración predeterminada de CU-01: doble 6, una CPU Intermedio y sin límite de tiempo.</summary>
    public static ConfiguracionPartida Predeterminada(string alias) =>
        new(alias, TipoDomino.Doble6, [NivelDificultad.Intermedio], LimiteTiempo.SinLimite);
}
