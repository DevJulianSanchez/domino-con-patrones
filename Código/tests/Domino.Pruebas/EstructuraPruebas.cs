using Domino.Dominio;
using Domino.Persistencia;

namespace Domino.Pruebas;

/// <summary>Facade y Adapter: el dominio funciona con cualquier implementación de sus abstracciones.</summary>
public class EstructuraPruebas
{
    /// <summary>Otra implementación del contrato, sin archivos ni JSON.</summary>
    private sealed class RepositorioEnMemoria : IRepositorioPartidas
    {
        private readonly Dictionary<string, PartidaGuardada> partidas = [];

        public void Guardar(string nombre, PartidaGuardada partida) => partidas[nombre] = partida;

        public PartidaGuardada Cargar(string nombre) => partidas[nombre];

        public IReadOnlyList<string> ListarNombres() => partidas.Keys.Order().ToList();
    }

    [Fact]
    public void El_controlador_guarda_y_carga_con_otro_repositorio_sin_cambios_en_el_dominio()
    {
        var controlador = new ControladorJuego(new RelojFalso(), new RepositorioEnMemoria(), new Random(5));
        var vista = controlador.IniciarPartida(Apoyo.Configuracion());
        for (var i = 0; i < 4 && vista.Estado != EstadoPartida.Terminada; i++)
            vista = Apoyo.JugarTurno(controlador, vista);

        controlador.GuardarPartida("memoria");

        Apoyo.AssertMismoEstado(vista, controlador.CargarPartida("memoria"));
        Assert.Equal(["memoria"], controlador.ListarPartidasGuardadas());
    }

    [Fact]
    public void El_adaptador_json_traduce_un_archivo_danado_a_una_excepcion_del_contrato()
    {
        var carpeta = Apoyo.CarpetaTemporal();
        Directory.CreateDirectory(carpeta);
        File.WriteAllText(Path.Combine(carpeta, "danada.json"), "{ esto no es una partida");

        var repositorio = new RepositorioPartidasJson(carpeta);

        Assert.Throws<InvalidDataException>(() => repositorio.Cargar("danada"));
        Assert.Throws<FileNotFoundException>(() => repositorio.Cargar("inexistente"));
        Assert.Throws<ArgumentException>(() => repositorio.Guardar("no/valido", null!));
    }

    [Fact]
    public void Sin_partida_el_controlador_rechaza_las_operaciones()
    {
        var controlador = Apoyo.Controlador(1);

        Assert.Throws<InvalidOperationException>(controlador.ObtenerVista);
        Assert.Throws<InvalidOperationException>(controlador.Robar);
    }
}
