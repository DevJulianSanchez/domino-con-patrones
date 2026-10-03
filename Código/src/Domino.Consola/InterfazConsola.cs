using Domino.Dominio;

namespace Domino.Consola;

/// <summary>Interfaz de texto: solo presenta la vista y traduce las acciones del jugador al controlador.</summary>
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
        var vista = controlador.IniciarPartida(LeerConfiguracion());
        Mostrar(vista);
        Console.WriteLine();
        Console.WriteLine("Partida configurada e iniciada. El ciclo de turnos aún no está implementado.");
    }

    /// <summary>CU-01 Configurar partida. Una respuesta vacía toma el valor predeterminado.</summary>
    private ConfiguracionPartida LeerConfiguracion()
    {
        string alias;
        while (true)
        {
            alias = Preguntar("Alias");
            try
            {
                ConfiguracionPartida.Predeterminada(alias).Validar();
                break;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  {ex.Message}");
            }
        }

        var tipo = Elegir("Tipo de dominó", [TipoDomino.Doble6, TipoDomino.Doble9], TipoDomino.Doble6);
        var cantidadCpu = Elegir("Número de CPU", [1, 2, 3], 1);
        var niveles = new List<NivelDificultad>();
        for (var i = 1; i <= cantidadCpu; i++)
            niveles.Add(Elegir($"Nivel de la CPU {i}", Enum.GetValues<NivelDificultad>(), NivelDificultad.Intermedio));
        var limite = Elegir("Límite de tiempo por turno", Enum.GetValues<LimiteTiempo>(), LimiteTiempo.SinLimite);

        return new ConfiguracionPartida(alias, tipo, niveles, limite);
    }

    private VistaPartida LeerAccion(VistaPartida vista) =>
        throw new NotImplementedException("Pendiente: leer y ejecutar la acción del jugador.");

    private void Mostrar(VistaPartida vista)
    {
        Console.WriteLine();
        Console.WriteLine($"Estado: {vista.Estado}");
        for (var i = 0; i < vista.Jugadores.Count; i++)
        {
            var marca = i == vista.PosicionEnTurno ? "> " : "  ";
            Console.WriteLine($"{marca}{vista.Jugadores[i],-25} {vista.FichasPorJugador[i],2} fichas");
        }
        Console.WriteLine($"Fichas para robar: {vista.FichasParaRobar}");
        Console.WriteLine($"Mesa: {(vista.Mesa.Count == 0 ? "(vacía)" : string.Join(" ", vista.Mesa.Select(f => $"[{f.ValorIzquierdo}|{f.ValorDerecho}]")))}");
        Console.WriteLine($"Tu mano: {string.Join(" ", vista.ManoPropia)}");
        if (vista.SegundosRestantes is { } segundos)
            Console.WriteLine($"Tiempo restante: {segundos} s");
        if (vista.EsTurnoPropio)
            for (var i = 0; i < vista.Opciones.Count; i++)
                Console.WriteLine($"  {i + 1}. {vista.Opciones[i].Ficha} en el extremo {vista.Opciones[i].Extremo}");
        if (vista.Resultado is { } resultado)
            Console.WriteLine(resultado.EsEmpate ? "Partida empatada." : $"Gana {resultado.Ganador!.Alias} ({resultado.Forma}).");
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
