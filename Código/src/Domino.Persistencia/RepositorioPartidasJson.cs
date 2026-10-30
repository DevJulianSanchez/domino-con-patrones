using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Domino.Dominio;

namespace Domino.Persistencia;

/// <summary>
/// Guarda cada partida como un archivo JSON dentro de una carpeta. Las jugadas son objetos de
/// distintas clases (Command); el discriminador se configura aquí para que el dominio no
/// dependa del formato del archivo.
/// </summary>
public sealed class RepositorioPartidasJson : IRepositorioPartidas
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { RegistrarTiposDeJugada } }
    };

    private readonly string carpeta;

    public RepositorioPartidasJson(string carpeta)
    {
        this.carpeta = carpeta;
    }

    public void Guardar(string nombre, PartidaGuardada partida)
    {
        Directory.CreateDirectory(carpeta);
        File.WriteAllText(Ruta(nombre), JsonSerializer.Serialize(partida, Opciones));
    }

    public PartidaGuardada Cargar(string nombre)
    {
        var ruta = Ruta(nombre);
        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe una partida guardada con el nombre '{nombre}'.", ruta);

        try
        {
            return JsonSerializer.Deserialize<PartidaGuardada>(File.ReadAllText(ruta), Opciones)
                   ?? throw new InvalidDataException($"El archivo '{ruta}' no contiene una partida válida.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"El archivo '{ruta}' no contiene una partida válida.", ex);
        }
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

    /// <summary>Usa el tipo de jugada como discriminador y omite la propiedad Tipo, que lo repetiría.</summary>
    private static void RegistrarTiposDeJugada(JsonTypeInfo info)
    {
        if (info.Type == typeof(Jugada))
        {
            info.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "tipo",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(JugadaColocarFicha), nameof(TipoJugada.ColocarFicha)),
                    new JsonDerivedType(typeof(JugadaRobar), nameof(TipoJugada.Robar)),
                    new JsonDerivedType(typeof(JugadaPasar), nameof(TipoJugada.Pasar)),
                    new JsonDerivedType(typeof(JugadaTiempoAgotado), nameof(TipoJugada.TiempoAgotado))
                }
            };
        }

        if (typeof(Jugada).IsAssignableFrom(info.Type) && info.Kind == JsonTypeInfoKind.Object)
        {
            var tipo = info.Properties.FirstOrDefault(p => p.Name == nameof(Jugada.Tipo));
            if (tipo is not null)
                info.Properties.Remove(tipo);
        }
    }
}
