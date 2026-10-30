using Domino.Dominio;

namespace Domino.Pruebas;

/// <summary>Command: las jugadas se deshacen y se vuelven a ejecutar al cargar.</summary>
public class JugadaPruebas
{
    [Fact]
    public void Deshacer_restaura_el_estado_previo_a_la_ultima_jugada_del_humano()
    {
        var probadas = 0;
        for (var semilla = 0; semilla < 60; semilla++)
        {
            var controlador = Apoyo.Controlador(semilla);
            var vista = Apoyo.AvanzarHasta(controlador, controlador.IniciarPartida(Apoyo.Configuracion()),
                v => v.Estado == EstadoPartida.EnCurso && v.FichasParaRobar > 0);
            if (vista is null)
                continue;

            var despues = Apoyo.JugarTurno(controlador, vista);
            if (despues.Estado == EstadoPartida.Terminada)
                continue;

            Apoyo.AssertMismoEstado(vista, controlador.DeshacerUltimaJugada());
            probadas++;
        }
        Assert.True(probadas > 30);
    }

    [Fact]
    public void Deshacer_la_salida_vuelve_al_estado_salida()
    {
        for (var semilla = 0; semilla < 100; semilla++)
        {
            var partida = new Partida(Apoyo.Configuracion(), semilla, new RelojFalso());
            partida.Iniciar();
            if (partida.JugadorEnTurno != partida.Humano)
                continue;

            var salida = Assert.Single(partida.OpcionesDe(partida.Humano));
            partida.JugarFicha(salida.Ficha, salida.Extremo);
            partida.ResolverTurnosCpu();
            partida.DeshacerUltimaJugadaHumano();

            Assert.Equal(EstadoPartida.Salida, partida.Estado);
            Assert.Equal(salida, Assert.Single(partida.OpcionesDe(partida.Humano)));
            return;
        }
        Assert.Fail("Ninguna semilla dejó la salida al humano.");
    }

    [Fact]
    public void Guardar_y_cargar_reconstruye_la_partida()
    {
        for (var semilla = 0; semilla < 30; semilla++)
        {
            var controlador = Apoyo.Controlador(semilla);
            var vista = controlador.IniciarPartida(Apoyo.Configuracion());
            for (var i = 0; i < 6 && vista.Estado != EstadoPartida.Terminada; i++)
                vista = Apoyo.JugarTurno(controlador, vista);
            if (vista.Estado == EstadoPartida.Terminada)
                continue;

            controlador.GuardarPartida("prueba");
            var cargada = controlador.CargarPartida("prueba");

            Apoyo.AssertMismoEstado(vista, cargada);
            Assert.Equal(
                vista.UltimasJugadas.Select(j => (j.Tipo, j.PosicionJugador, j.Orden)),
                cargada.UltimasJugadas.Select(j => (j.Tipo, j.PosicionJugador, j.Orden)));
        }
    }

    [Fact]
    public void Guardar_y_cargar_despues_de_deshacer_conserva_las_fichas_robadas()
    {
        var probadas = 0;
        for (var semilla = 0; semilla < 60; semilla++)
        {
            var controlador = Apoyo.Controlador(semilla);
            var vista = Apoyo.AvanzarHasta(controlador, controlador.IniciarPartida(Apoyo.Configuracion()),
                v => v.PuedeRobar && v.PuedeDeshacer);
            if (vista is null)
                continue;

            vista = controlador.Robar();
            if (!vista.PuedeDeshacer)
                continue;
            vista = controlador.DeshacerUltimaJugada();
            controlador.GuardarPartida("robo");

            Apoyo.AssertMismoEstado(vista, controlador.CargarPartida("robo"));
            probadas++;
        }
        Assert.True(probadas > 0);
    }
}
