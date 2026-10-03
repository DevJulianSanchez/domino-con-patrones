using Domino.Dominio;

namespace Domino.Consola;

/// <summary>Interfaz de texto: solo presenta la información y traduce las acciones del jugador al controlador.</summary>
public sealed class InterfazConsola
{
    private readonly ControladorJuego controlador;

    public InterfazConsola(ControladorJuego controlador)
    {
        this.controlador = controlador;
    }

    public void Ejecutar()
    {
        Console.WriteLine("=== Dominó con patrones ===");
        Console.WriteLine();
        var partida = controlador.CrearPartida(LeerConfiguracion());

        Console.WriteLine();
        Console.WriteLine($"Partida de {partida.Tipo} creada ({partida.Estado}). Jugadores:");
        foreach (var jugador in partida.Jugadores)
            Console.WriteLine($"  {jugador.Posicion}. {jugador.Alias}");
        Console.WriteLine();
        Console.WriteLine("El reparto y el ciclo de turnos aún no están implementados.");
    }

    /// <summary>CU-01 Configurar partida. Una respuesta vacía toma el valor predeterminado.</summary>
    private static ConfiguracionPartida LeerConfiguracion()
    {
        while (true)
        {
            var alias = Preguntar("Alias");
            var tipo = Elegir("Tipo de dominó", [TipoDomino.Doble6, TipoDomino.Doble9], TipoDomino.Doble6);
            var cantidadCpu = Elegir("Número de CPU", [1, 2, 3], 1);
            var niveles = new List<NivelDificultad>();
            for (var i = 1; i <= cantidadCpu; i++)
                niveles.Add(Elegir($"Nivel de la CPU {i}", Enum.GetValues<NivelDificultad>(), NivelDificultad.Intermedio));

            var configuracion = new ConfiguracionPartida(alias, tipo, niveles);
            try
            {
                configuracion.Validar();
                return configuracion;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  {ex.Message}");
            }
        }
    }

    private static string Preguntar(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static T Elegir<T>(string etiqueta, IReadOnlyList<T> opciones, T predeterminado)
    {
        while (true)
        {
            var lista = string.Join(", ", opciones.Select((o, i) => $"{i + 1}={o}"));
            var respuesta = Preguntar($"{etiqueta} ({lista}) [{predeterminado}]");
            if (respuesta.Length == 0)
                return predeterminado;
            if (int.TryParse(respuesta, out var n) && n >= 1 && n <= opciones.Count)
                return opciones[n - 1];
            Console.WriteLine("  Opción no válida.");
        }
    }
}
