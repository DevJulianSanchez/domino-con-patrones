using Domino.Dominio;

namespace Domino.Pruebas;

public class PartidaCompletaPruebas
{
    public static TheoryData<TipoDomino, int> Configuraciones() => new()
    {
        { TipoDomino.Doble6, 1 }, { TipoDomino.Doble6, 2 }, { TipoDomino.Doble6, 3 },
        { TipoDomino.Doble9, 1 }, { TipoDomino.Doble9, 2 }, { TipoDomino.Doble9, 3 }
    };

    [Theory]
    [MemberData(nameof(Configuraciones))]
    public void Las_partidas_terminan_y_conservan_las_fichas(TipoDomino tipo, int cpus)
    {
        var total = DefinicionTipoDomino.De(tipo).TotalFichas;
        for (var semilla = 0; semilla < 100; semilla++)
        {
            var controlador = Apoyo.Controlador(semilla);
            var vista = controlador.IniciarPartida(Apoyo.Configuracion(tipo, cpus));
            var pasos = 0;
            while (vista.Estado != EstadoPartida.Terminada)
            {
                Assert.True(vista.EsTurnoPropio, "Tras cada acción el turno debe volver al humano.");
                Assert.Equal(total, Apoyo.TotalFichas(vista));
                vista = Apoyo.JugarTurno(controlador, vista);
                Assert.True(++pasos < 200, "La partida no termina.");
            }

            Assert.Equal(total, Apoyo.TotalFichas(vista));
            var resultado = vista.Resultado!;
            if (resultado.Forma == FormaDeCierre.Domino)
                Assert.Equal(0, resultado.Ganador!.CantidadFichas);
            else
                Assert.Equal(0, vista.FichasParaRobar);
        }
    }

    [Fact]
    public void Con_doble_6_y_cuatro_jugadores_abre_el_doble_6()
    {
        for (var semilla = 0; semilla < 50; semilla++)
        {
            var partida = new Partida(Apoyo.Configuracion(TipoDomino.Doble6, 3), semilla, new RelojFalso());
            partida.Iniciar();

            Assert.Equal(EstadoPartida.Salida, partida.Estado);
            var opcion = Assert.Single(partida.OpcionesDe(partida.JugadorEnTurno));
            Assert.Equal(new Ficha(6, 6), opcion.Ficha);
        }
    }

    [Fact]
    public void La_misma_semilla_reproduce_la_misma_partida()
    {
        static IReadOnlyList<FichaColocada> Jugar(int semilla)
        {
            var controlador = Apoyo.Controlador(semilla);
            var vista = controlador.IniciarPartida(Apoyo.Configuracion());
            return Apoyo.AvanzarHasta(controlador, vista, _ => false) is null ? controlador.ObtenerVista().Mesa : [];
        }

        Assert.Equal(Jugar(7), Jugar(7));
    }
}
