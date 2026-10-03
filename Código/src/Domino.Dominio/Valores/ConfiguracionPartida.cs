namespace Domino.Dominio;

/// <summary>Datos elegidos por el jugador en CU-01 Configurar partida.</summary>
public sealed record ConfiguracionPartida(string Alias, TipoDomino Tipo, IReadOnlyList<NivelDificultad> NivelesCpu)
{
    public const int LongitudMaximaAlias = 15;

    /// <summary>Verifica RN-02 y el alias de CU-01.</summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Alias))
            throw new ArgumentException("El alias no puede estar vacío.");
        if (Alias.Trim().Length > LongitudMaximaAlias)
            throw new ArgumentException($"El alias no puede superar {LongitudMaximaAlias} caracteres.");
        if (NivelesCpu.Count is < 1 or > 3)
            throw new ArgumentException("La partida admite de una a tres CPU.");
    }
}
