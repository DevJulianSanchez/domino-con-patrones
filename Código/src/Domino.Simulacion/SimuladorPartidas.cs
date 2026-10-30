using Domino.Dominio;
using Domino.Persistencia;

namespace Domino.Simulacion;

/// <summary>
/// Ejecuta partidas completas sin interfaz y con semilla fija. Ocupa el lugar del jugador humano
/// y juega la primera opción válida, o roba o pasa cuando no la tiene. En cada partida ejercita
/// también deshacer, guardar y cargar, que forman parte de las acciones críticas medidas.
/// </summary>
public sealed class SimuladorPartidas
{
    /// <summary>Turno del humano en el que se deshace, se guarda y se carga la partida.</summary>
    private const int TurnoDeGuardado = 3;

    private readonly ConfiguracionPartida configuracion;
    private readonly string carpeta;

    public SimuladorPartidas(ConfiguracionPartida configuracion, string carpeta)
    {
        this.configuracion = configuracion;
        this.carpeta = carpeta;
    }

    /// <summary>Juega las partidas y devuelve cuántas ganó cada alias; las empatadas se cuentan como "Empate".</summary>
    public IReadOnlyDictionary<string, int> Ejecutar(int partidas, int semilla)
    {
        var victorias = new Dictionary<string, int>();
        var semillas = new Random(semilla);
        for (var i = 0; i < partidas; i++)
        {
            var resultado = JugarPartida(semillas.Next());
            var clave = resultado.Ganador?.Alias ?? "Empate";
            victorias[clave] = victorias.GetValueOrDefault(clave) + 1;
        }
        return victorias;
    }

    private Resultado JugarPartida(int semilla)
    {
        var controlador = new ControladorJuego(
            new RelojSimulado(new DateTime(2026, 1, 1)), new RepositorioPartidasJson(carpeta), new Random(semilla));
        var vista = controlador.IniciarPartida(configuracion);
        var turno = 0;
        while (vista.Estado != EstadoPartida.Terminada)
        {
            vista = JugarTurnoHumano(controlador, vista);
            if (++turno == TurnoDeGuardado && vista.PuedeDeshacer)
                vista = DeshacerGuardarYCargar(controlador);
        }
        return vista.Resultado!;
    }

    private static VistaPartida JugarTurnoHumano(ControladorJuego controlador, VistaPartida vista)
    {
        if (vista.Opciones.Count > 0)
            return controlador.JugarFicha(vista.Opciones[0].Ficha, vista.Opciones[0].Extremo);
        return vista.PuedeRobar ? controlador.Robar() : controlador.Pasar();
    }

    private static VistaPartida DeshacerGuardarYCargar(ControladorJuego controlador)
    {
        controlador.DeshacerUltimaJugada();
        controlador.GuardarPartida("simulacion");
        return controlador.CargarPartida("simulacion");
    }
}
