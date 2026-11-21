namespace Domino.Dominio;

/// <summary>
/// Datos elegidos por el jugador en CU-01 Configurar partida. Se arma paso a paso con
/// ConstructorConfiguracion; también se reconstruye al cargar una partida guardada.
/// </summary>
public sealed record ConfiguracionPartida(
    string Alias,
    TipoDomino Tipo,
    IReadOnlyList<NivelDificultad> NivelesCpu,
    LimiteTiempo Limite)
{
    public const int LongitudMaximaAlias = 15;

    public const int MinimoCpu = 1;

    public const int MaximoCpu = 3;

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
        ValidarAlias(Alias);
        if (NivelesCpu.Count is < MinimoCpu or > MaximoCpu)
            throw new ArgumentException("La partida admite de una a tres CPU.");
    }

    /// <summary>CU-01, curso alterno 2a: el alias no puede estar vacío ni superar la longitud permitida.</summary>
    public static void ValidarAlias(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException("El alias no puede estar vacío.");
        if (alias.Trim().Length > LongitudMaximaAlias)
            throw new ArgumentException($"El alias no puede superar {LongitudMaximaAlias} caracteres.");
    }
}
