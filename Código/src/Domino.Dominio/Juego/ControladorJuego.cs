namespace Domino.Dominio;

/// <summary>Punto de entrada de las operaciones del sistema que llegan desde la interfaz.</summary>
public sealed class ControladorJuego
{
    public Partida CrearPartida(ConfiguracionPartida configuracion) => new(configuracion);
}
