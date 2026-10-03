using Domino.Dominio;
using Domino.Simulacion;

// Uso: dotnet run --project src/Domino.Simulacion -- [partidas] [semilla]
var partidas = args.Length > 0 ? int.Parse(args[0]) : 1000;
var semilla = args.Length > 1 ? int.Parse(args[1]) : 42;
var configuracion = new ConfiguracionPartida(
    "Simulado", TipoDomino.Doble9,
    [NivelDificultad.Principiante, NivelDificultad.Intermedio, NivelDificultad.Avanzado],
    LimiteTiempo.SinLimite);
var carpeta = Path.Combine(Path.GetTempPath(), "DominoPatrones", "simulacion");

Console.WriteLine($"Simulando {partidas} partidas con semilla {semilla}...");
new SimuladorPartidas(configuracion, carpeta).Ejecutar(partidas, semilla);
Console.WriteLine("Simulación terminada.");
