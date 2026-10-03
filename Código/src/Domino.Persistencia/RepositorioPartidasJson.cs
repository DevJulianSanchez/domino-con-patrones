using System.Text.Json;
using System.Text.Json.Serialization;
using Domino.Dominio;

namespace Domino.Persistencia;

/// <summary>Guarda cada partida como un archivo JSON dentro de una carpeta.</summary>
public sealed class RepositorioPartidasJson : IRepositorioPartidas
{
    private readonly string carpeta;

    public RepositorioPartidasJson(string carpeta)
    {
        this.carpeta = carpeta;
    }

    public void Guardar(string nombre, PartidaGuardada partida)
    {
        Directory.CreateDirectory(carpeta);
        var opciones = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
        File.WriteAllText(Ruta(nombre), JsonSerializer.Serialize(partida, opciones));
    }

    public PartidaGuardada Cargar(string nombre)
    {
        var ruta = Ruta(nombre);
        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe una partida guardada con el nombre '{nombre}'.", ruta);

        var opciones = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        return JsonSerializer.Deserialize<PartidaGuardada>(File.ReadAllText(ruta), opciones)
               ?? throw new InvalidDataException($"El archivo '{ruta}' no contiene una partida válida.");
    }

    public IReadOnlyList<string> ListarNombres() =>
        Directory.Exists(carpeta)
            ? Directory.EnumerateFiles(carpeta, "*.json").Select(Path.GetFileNameWithoutExtension).OfType<string>().Order().ToList()
            : [];

    private string Ruta(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException($"'{nombre}' no es un nombre válido para guardar la partida.", nameof(nombre));
        return Path.Combine(carpeta, nombre + ".json");
    }
}
