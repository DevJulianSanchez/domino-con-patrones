namespace Domino.Dominio;

/// <summary>
/// State: comportamiento de la partida que depende de su estado. Cada estado decide qué
/// jugadas admite, qué opciones ofrece, si corre el tiempo del turno y si se puede deshacer;
/// por defecto no admite nada. La partida delega en su estado actual y el estado decide
/// la transición.
/// </summary>
internal abstract class EstadoJuego
{
    public abstract EstadoPartida Valor { get; }

    /// <summary>Indica si la partida admite jugadas en este estado.</summary>
    public virtual bool EnJuego => false;

    public virtual bool PermiteDeshacer => false;

    public virtual bool ControlaTiempo => false;

    public virtual void Iniciar(Partida partida) =>
        throw new InvalidOperationException("La partida ya fue iniciada.");

    public virtual IReadOnlyList<OpcionJugada> OpcionesDe(Partida partida, Jugador jugador) => [];

    public virtual void Procesar(Partida partida, Jugada jugada) =>
        throw new InvalidOperationException($"La partida no admite jugadas en el estado {Valor}.");

    /// <summary>Registra la pérdida del turno si el tiempo se agotó (RN-04). Indica si ocurrió.</summary>
    public virtual bool VerificarTiempo(Partida partida) => false;

    public virtual void Deshacer(Partida partida) =>
        throw new InvalidOperationException($"No se puede deshacer en el estado {Valor}.");
}
