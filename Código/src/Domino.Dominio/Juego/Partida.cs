namespace Domino.Dominio;

/// <summary>Aplica las reglas del juego. Por ahora solo crea los jugadores a partir de la configuración.</summary>
public sealed class Partida
{
    private readonly ConfiguracionPartida configuracion;
    private readonly List<Jugador> jugadores = [];
    private EstadoPartida estado = EstadoPartida.Preparada;

    public Partida(ConfiguracionPartida configuracion)
    {
        configuracion.Validar();
        this.configuracion = configuracion;
        CrearJugadores();
    }

    public EstadoPartida Estado => estado;

    public TipoDomino Tipo => configuracion.Tipo;

    public IReadOnlyList<Jugador> Jugadores => jugadores;

    public void Iniciar() => throw new NotImplementedException("Pendiente: repartir las fichas y determinar la salida.");

    /// <summary>El humano ocupa la posición 0 y las CPU las siguientes (RN-02).</summary>
    private void CrearJugadores()
    {
        jugadores.Add(new JugadorHumano(configuracion.Alias.Trim(), 0));
        for (var i = 0; i < configuracion.NivelesCpu.Count; i++)
        {
            var posicion = i + 1;
            var nivel = configuracion.NivelesCpu[i];
            jugadores.Add(new JugadorCpu($"CPU {posicion} ({nivel})", posicion, nivel));
        }
    }
}
