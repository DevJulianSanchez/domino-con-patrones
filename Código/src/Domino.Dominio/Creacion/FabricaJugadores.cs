namespace Domino.Dominio;

/// <summary>
/// Factory Method, en su variante parametrizada: CrearEstrategia recibe el nivel y decide qué
/// estrategia concreta instanciar. Concentra en un solo lugar el conocimiento de las estrategias
/// concretas y la forma de sembrar la aleatoria del nivel Principiante, para que la partida
/// reciba los jugadores ya armados y solo dependa de IEstrategiaCpu.
/// </summary>
public sealed class FabricaJugadores
{
    private readonly int semilla;

    public FabricaJugadores(int semilla)
    {
        this.semilla = semilla;
    }

    /// <summary>El humano ocupa la posición 0 y las CPU las siguientes, con la estrategia de su nivel (RN-02).</summary>
    public List<Jugador> CrearJugadores(ConfiguracionPartida configuracion)
    {
        var jugadores = new List<Jugador> { new JugadorHumano(configuracion.Alias.Trim(), 0) };
        for (var i = 0; i < configuracion.NivelesCpu.Count; i++)
            jugadores.Add(CrearCpu(configuracion.NivelesCpu[i], posicion: i + 1));
        return jugadores;
    }

    public JugadorCpu CrearCpu(NivelDificultad nivel, int posicion) =>
        new($"CPU {posicion} ({nivel})", posicion, CrearEstrategia(nivel, posicion));

    /// <summary>
    /// Método de fabricación. La aleatoria del nivel Principiante se deriva de la semilla de la
    /// partida y de la posición, para que una misma semilla produzca siempre la misma partida.
    /// </summary>
    public IEstrategiaCpu CrearEstrategia(NivelDificultad nivel, int posicion) => nivel switch
    {
        NivelDificultad.Principiante => new EstrategiaPrincipiante(new Random(semilla + posicion)),
        NivelDificultad.Intermedio => new EstrategiaIntermedia(),
        NivelDificultad.Avanzado => new EstrategiaAvanzada(),
        _ => throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nivel de dificultad no soportado.")
    };
}
