using System.Globalization;
using System.Runtime.InteropServices;
using Domino.Dominio;
using Domino.Simulacion;

// Uso:
//   dotnet run -c Release --project src/Domino.Simulacion -- [partidas] [semilla]
//   dotnet run -c Release --project src/Domino.Simulacion -- banco [partidas] [semilla] [corridas] [archivo.csv]
var configuracion = new ConstructorConfiguracion()
    .ConAlias("Simulado")
    .ConTipo(TipoDomino.Doble9)
    .AgregarCpu(NivelDificultad.Principiante)
    .AgregarCpu(NivelDificultad.Intermedio)
    .AgregarCpu(NivelDificultad.Avanzado)
    .Construir();
var carpeta = Path.Combine(Path.GetTempPath(), "DominoPatrones", "simulacion");

if (args.Length > 0 && args[0] == "banco")
{
    EjecutarBanco(args[1..]);
    return;
}

var partidas = args.Length > 0 ? int.Parse(args[0]) : 1000;
var semilla = args.Length > 1 ? int.Parse(args[1]) : 42;
Console.WriteLine($"Simulando {partidas} partidas con semilla {semilla}...");
var victorias = new SimuladorPartidas(configuracion, carpeta).Ejecutar(partidas, semilla);
Console.WriteLine("Simulación terminada. Partidas ganadas:");
ImprimirVictorias(victorias);

void EjecutarBanco(string[] opciones)
{
    var partidas = opciones.Length > 0 ? int.Parse(opciones[0]) : 1000;
    var semilla = opciones.Length > 1 ? int.Parse(opciones[1]) : 42;
    var corridas = opciones.Length > 2 ? int.Parse(opciones[2]) : 5;
    var archivo = opciones.Length > 3 ? opciones[3] : null;
    var calentamiento = Math.Max(1, partidas / 5);

    var banco = new BancoPruebas(configuracion, carpeta);
    Console.WriteLine($".NET {Environment.Version} · {RuntimeInformation.OSDescription} · {RuntimeInformation.ProcessArchitecture} · " +
                      $"{Environment.ProcessorCount} núcleos · GC servidor: {System.Runtime.GCSettings.IsServerGC}");
    Console.WriteLine($"Calentamiento: {calentamiento} partidas descartadas. Medición: {corridas} corridas de {partidas} partidas, semilla {semilla}.");
    banco.Ejecutar(calentamiento, semilla + 1);

    var porCorrida = new List<IReadOnlyDictionary<string, Registro>>();
    var memoriaTrasGc = new List<long>();
    IReadOnlyDictionary<string, int> victoriasBanco = new Dictionary<string, int>();
    for (var c = 0; c < corridas; c++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var (registros, victorias) = banco.Ejecutar(partidas, semilla);
        porCorrida.Add(registros.ToDictionary());
        victoriasBanco = victorias;
        memoriaTrasGc.Add(GC.GetTotalMemory(forceFullCollection: true));
    }

    // Cada cifra es la mediana de las corridas (Anexo C, regla 2).
    double Mediana(IEnumerable<double> valores)
    {
        var o = valores.Order().ToList();
        return o.Count % 2 == 1 ? o[o.Count / 2] : (o[o.Count / 2 - 1] + o[o.Count / 2]) / 2;
    }

    var acciones = new[]
    {
        BancoPruebas.EjecutarJugada, BancoPruebas.CalcularOpciones,
        BancoPruebas.DecidirJugada(NivelDificultad.Principiante), BancoPruebas.DecidirJugada(NivelDificultad.Intermedio),
        BancoPruebas.DecidirJugada(NivelDificultad.Avanzado), BancoPruebas.Repartir,
        BancoPruebas.Deshacer, BancoPruebas.Guardar, BancoPruebas.Cargar,
        BancoPruebas.VistaCpu, BancoPruebas.LeerArchivo, BancoPruebas.RepetirJugadas
    };
    var inv = CultureInfo.InvariantCulture;
    var lineas = new List<string> { "accion,invocaciones,p50_us,p95_us,cpu_reloj,bytes_p50,reloj_total_ms" };
    Console.WriteLine();
    Console.WriteLine($"{"Acción crítica",-34}{"n",9}{"p50 µs",10}{"p95 µs",10}{"CPU/reloj",11}{"B p50",9}{"total ms",10}");
    foreach (var accion in acciones)
    {
        var muestras = porCorrida.Select(r => r[accion]).ToList();
        var n = muestras[0].Cantidad;
        var p50 = Mediana(muestras.Select(m => m.PercentilMicrosegundos(50)));
        var p95 = Mediana(muestras.Select(m => m.PercentilMicrosegundos(95)));
        var ratio = Mediana(muestras.Select(m => m.CpuTotal / m.RelojTotal));
        var bytes = Mediana(muestras.Select(m => m.PercentilBytes(50)));
        var total = Mediana(muestras.Select(m => m.RelojTotal.TotalMilliseconds));
        Console.WriteLine($"{accion,-34}{n,9}{p50,10:F2}{p95,10:F2}{ratio,11:F2}{bytes,9:F0}{total,10:F1}");
        lineas.Add(string.Join(",", $"\"{accion}\"", n, p50.ToString("F2", inv), p95.ToString("F2", inv),
            ratio.ToString("F2", inv), bytes.ToString("F0", inv), total.ToString("F1", inv)));
    }

    var memoria = Mediana(memoriaTrasGc.Select(m => (double)m)) / 1024;
    Console.WriteLine();
    Console.WriteLine($"Memoria administrada tras la recolección (mediana): {memoria:F0} KiB");
    Console.WriteLine("Partidas ganadas en la última corrida (deben coincidir con el simulador):");
    ImprimirVictorias(victoriasBanco);

    if (archivo is not null)
    {
        lineas.Add($"\"Memoria tras GC (KiB)\",,,,,{memoria.ToString("F0", inv)},");
        File.WriteAllLines(archivo, lineas);
        Console.WriteLine($"Resultados en {archivo}");
    }
}

static void ImprimirVictorias(IReadOnlyDictionary<string, int> victorias)
{
    foreach (var (alias, cantidad) in victorias.OrderByDescending(v => v.Value))
        Console.WriteLine($"  {alias,-25} {cantidad,5}");
}
