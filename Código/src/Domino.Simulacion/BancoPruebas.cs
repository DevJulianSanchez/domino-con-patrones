using System.Diagnostics;
using Domino.Dominio;
using Domino.Persistencia;

namespace Domino.Simulacion;

/// <summary>
/// Modo banco de pruebas del Anexo C. Juega el mismo escenario que SimuladorPartidas, pero maneja
/// la partida directamente para separar las siete acciones críticas declaradas, y toma de cada
/// invocación el tiempo de reloj, el tiempo de CPU del hilo y los bytes asignados. No imprime dentro del
/// bucle medido. El dominio no se instrumenta: la medición está toda en esta clase.
/// </summary>
public sealed class BancoPruebas
{
    public const string EjecutarJugada = "Ejecutar jugada";
    public const string CalcularOpciones = "Calcular jugadas válidas";
    public const string Repartir = "Repartir fichas";
    public const string Deshacer = "Deshacer jugada";
    public const string Guardar = "Guardar partida";
    public const string Cargar = "Cargar partida";

    // Mediciones auxiliares para el costo de los patrones; no son acciones críticas.
    public const string VistaCpu = "Construir vista (Facade)";
    public const string LeerArchivo = "Cargar: leer JSON (Adapter)";
    public const string RepetirJugadas = "Cargar: repetir jugadas (Command)";

    /// <summary>Acción del humano simulado tras la cual se deshace, se guarda y se carga, como en el simulador.</summary>
    private const int TurnoDeGuardado = 3;

    private readonly ConfiguracionPartida configuracion;
    private readonly RepositorioPartidasJson repositorio;
    private readonly Dictionary<string, Registro> registros = [];

    public BancoPruebas(ConfiguracionPartida configuracion, string carpeta)
    {
        this.configuracion = configuracion;
        repositorio = new RepositorioPartidasJson(carpeta);
    }

    public static string DecidirJugada(NivelDificultad nivel) => $"Decidir jugada ({nivel})";

    /// <summary>Juega las partidas y devuelve las muestras de cada acción y las victorias por alias.</summary>
    public (IReadOnlyDictionary<string, Registro> Registros, IReadOnlyDictionary<string, int> Victorias) Ejecutar(int partidas, int semilla)
    {
        registros.Clear();
        var victorias = new Dictionary<string, int>();
        var semillas = new Random(semilla);
        for (var i = 0; i < partidas; i++)
        {
            // Misma derivación de semillas que SimuladorPartidas y ControladorJuego.
            var resultado = JugarPartida(new Random(semillas.Next()).Next());
            var clave = resultado.Ganador?.Alias ?? "Empate";
            victorias[clave] = victorias.GetValueOrDefault(clave) + 1;
        }
        return (registros, victorias);
    }

    private Resultado JugarPartida(int semilla)
    {
        var reloj = new RelojSimulado(new DateTime(2026, 1, 1));
        var partida = Medir(Repartir, () =>
        {
            var nueva = new Partida(configuracion, semilla, reloj);
            nueva.Iniciar();
            return nueva;
        });

        var accionesHumano = 0;
        var guardadoPendiente = false;
        while (partida.Estado != EstadoPartida.Terminada)
        {
            var jugador = partida.JugadorEnTurno;
            if (jugador == partida.Humano && guardadoPendiente)
            {
                guardadoPendiente = false;
                if (partida.ObtenerVistaPara(jugador).PuedeDeshacer)
                {
                    partida = DeshacerGuardarYCargar(partida, reloj);
                    continue;
                }
            }

            var opciones = Medir(CalcularOpciones, () => partida.OpcionesDe(jugador));
            OpcionJugada? elegida = null;
            if (opciones.Count > 0)
            {
                if (jugador is JugadorCpu cpu)
                {
                    var vista = Medir(VistaCpu, () => partida.ObtenerVistaPara(cpu));
                    elegida = Medir(DecidirJugada(cpu.Nivel), () => cpu.DecidirJugada(opciones, vista));
                }
                else
                    elegida = opciones[0];
            }

            var quedanParaRobar = partida.ObtenerVistaPara(jugador).FichasParaRobar > 0;
            Medir(EjecutarJugada, () =>
            {
                if (elegida is not null)
                    partida.JugarFicha(elegida.Ficha, elegida.Extremo);
                else if (quedanParaRobar)
                    partida.Robar();
                else
                    partida.Pasar();
                return true;
            });

            if (jugador == partida.Humano && ++accionesHumano == TurnoDeGuardado)
                guardadoPendiente = true;
        }
        return partida.ObtenerVistaPara(partida.Humano).Resultado!;
    }

    private Partida DeshacerGuardarYCargar(Partida partida, IReloj reloj)
    {
        Medir(Deshacer, () =>
        {
            partida.DeshacerUltimaJugadaHumano();
            return true;
        });
        Medir(Guardar, () =>
        {
            repositorio.Guardar("banco", partida.CrearGuardado());
            return true;
        });
        return Medir(Cargar, () =>
        {
            var guardada = Medir(LeerArchivo, () => repositorio.Cargar("banco"));
            return Medir(RepetirJugadas, () => Partida.Restaurar(guardada, reloj));
        });
    }

    /// <summary>
    /// Toma el tiempo de reloj, el de CPU del hilo y los bytes asignados por el hilo durante la
    /// acción. El reloj se lee por dentro de las lecturas de CPU para no sumarles su costo.
    /// </summary>
    private T Medir<T>(string accion, Func<T> funcion)
    {
        var cpuInicio = RelojCpu.HiloActual();
        var bytesInicio = GC.GetAllocatedBytesForCurrentThread();
        var inicio = Stopwatch.GetTimestamp();
        var valor = funcion();
        var fin = Stopwatch.GetTimestamp();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - bytesInicio;
        var cpu = RelojCpu.HiloActual() - cpuInicio;

        if (!registros.TryGetValue(accion, out var registro))
            registros[accion] = registro = new Registro();
        registro.Agregar(Stopwatch.GetElapsedTime(inicio, fin), cpu, bytes);
        return valor;
    }
}

/// <summary>Muestras de una acción crítica en una corrida.</summary>
public sealed class Registro
{
    private readonly List<double> microsegundos = [];
    private readonly List<long> bytes = [];

    public int Cantidad => microsegundos.Count;

    public TimeSpan RelojTotal { get; private set; }

    public TimeSpan CpuTotal { get; private set; }

    public void Agregar(TimeSpan reloj, TimeSpan cpu, long asignados)
    {
        microsegundos.Add(reloj.TotalMicroseconds);
        bytes.Add(asignados);
        RelojTotal += reloj;
        CpuTotal += cpu;
    }

    public double PercentilMicrosegundos(double p) => Percentil(microsegundos, p);

    public double PercentilBytes(double p) => Percentil(bytes.Select(b => (double)b).ToList(), p);

    /// <summary>Percentil por el método del rango más cercano.</summary>
    private static double Percentil(List<double> valores, double p)
    {
        if (valores.Count == 0)
            return 0;
        var ordenados = valores.Order().ToList();
        var indice = (int)Math.Ceiling(p / 100 * ordenados.Count) - 1;
        return ordenados[Math.Clamp(indice, 0, ordenados.Count - 1)];
    }
}
