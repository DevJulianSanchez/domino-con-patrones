namespace Domino.Dominio;

/// <summary>
/// Builder: arma la configuración de CU-01 paso a paso. Parte de los valores predeterminados,
/// valida cada paso en el momento en que ocurre (el alias en el paso 2, el máximo de CPU de
/// RN-02 al agregar cada una) y entrega la configuración solo cuando está completa. Quien dirige
/// los pasos (la consola, el simulador o una prueba) no conoce las reglas de la configuración.
/// </summary>
public sealed class ConstructorConfiguracion
{
    public const TipoDomino TipoPredeterminado = TipoDomino.Doble6;

    public const NivelDificultad NivelPredeterminado = NivelDificultad.Intermedio;

    public const LimiteTiempo LimitePredeterminado = LimiteTiempo.SinLimite;

    private readonly List<NivelDificultad> nivelesCpu = [];
    private string? alias;
    private TipoDomino tipo = TipoPredeterminado;
    private LimiteTiempo limite = LimitePredeterminado;

    /// <summary>Cantidades de CPU que admite RN-02, para que la interfaz las ofrezca sin conocer la regla.</summary>
    public static IReadOnlyList<int> CantidadesCpu { get; } =
        Enumerable.Range(ConfiguracionPartida.MinimoCpu, ConfiguracionPartida.MaximoCpu - ConfiguracionPartida.MinimoCpu + 1).ToList();

    public int CantidadCpu => nivelesCpu.Count;

    /// <summary>Paso 1 de CU-01. Valida el alias de inmediato (curso alterno 2a).</summary>
    public ConstructorConfiguracion ConAlias(string alias)
    {
        ConfiguracionPartida.ValidarAlias(alias);
        this.alias = alias.Trim();
        return this;
    }

    public ConstructorConfiguracion ConTipo(TipoDomino tipo)
    {
        DefinicionTipoDomino.De(tipo);
        this.tipo = tipo;
        return this;
    }

    /// <summary>Agrega una CPU con su nivel; rechaza la que supere el máximo de RN-02.</summary>
    public ConstructorConfiguracion AgregarCpu(NivelDificultad nivel)
    {
        if (nivelesCpu.Count == ConfiguracionPartida.MaximoCpu)
            throw new ArgumentException($"La partida admite como máximo {ConfiguracionPartida.MaximoCpu} CPU.");
        if (!Enum.IsDefined(nivel))
            throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nivel de dificultad no soportado.");
        nivelesCpu.Add(nivel);
        return this;
    }

    public ConstructorConfiguracion ConLimite(LimiteTiempo limite)
    {
        if (!Enum.IsDefined(limite))
            throw new ArgumentOutOfRangeException(nameof(limite), limite, "Límite de tiempo no soportado.");
        this.limite = limite;
        return this;
    }

    /// <summary>
    /// Entrega la configuración. El alias es obligatorio; si no se agregó ninguna CPU, se usa la
    /// configuración predeterminada de CU-01: una CPU de nivel Intermedio.
    /// </summary>
    public ConfiguracionPartida Construir()
    {
        if (alias is null)
            throw new InvalidOperationException("Falta el alias del jugador.");

        var niveles = nivelesCpu.Count > 0 ? nivelesCpu.ToList() : [NivelPredeterminado];
        var configuracion = new ConfiguracionPartida(alias, tipo, niveles, limite);
        configuracion.Validar();
        return configuracion;
    }
}
