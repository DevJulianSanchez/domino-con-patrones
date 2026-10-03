using Domino.Dominio;
using Domino.Persistencia;

namespace Domino.Simulacion;

/// <summary>
/// Ejecuta partidas completas sin interfaz y con semilla fija. Ocupa el lugar del jugador humano
/// y juega la primera opción válida, o roba o pasa cuando no la tiene.
/// </summary>
public sealed class SimuladorPartidas
{
    private readonly ConfiguracionPartida configuracion;
    private readonly string carpeta;

    public SimuladorPartidas(ConfiguracionPartida configuracion, string carpeta)
    {
        this.configuracion = configuracion;
        this.carpeta = carpeta;
    }

    public void Ejecutar(int partidas, int semilla)
    {
        var semillas = new Random(semilla);
        for (var i = 0; i < partidas; i++)
            JugarPartida(semillas.Next());
    }

    private void JugarPartida(int semilla)
    {
        var controlador = new ControladorJuego(
            new RelojSimulado(new DateTime(2026, 1, 1)), new RepositorioPartidasJson(carpeta), new Random(semilla));
        var vista = controlador.IniciarPartida(configuracion);
        while (vista.Estado != EstadoPartida.Terminada)
            vista = JugarTurnoHumano(controlador, vista);
    }

    private VistaPartida JugarTurnoHumano(ControladorJuego controlador, VistaPartida vista)
    {
        if (vista.Opciones.Count > 0)
            return controlador.JugarFicha(vista.Opciones[0].Ficha, vista.Opciones[0].Extremo);
        return vista.PuedeRobar ? controlador.Robar() : controlador.Pasar();
    }
}
