using Domino.Dominio;

namespace Domino.Pruebas;

/// <summary>State: las acciones admitidas dependen del estado de la partida.</summary>
public class EstadoPruebas
{
    [Fact]
    public void Preparada_no_admite_jugadas()
    {
        var partida = new Partida(Apoyo.Configuracion(), 1, new RelojFalso());

        Assert.Equal(EstadoPartida.Preparada, partida.Estado);
        Assert.Throws<InvalidOperationException>(() => partida.Robar());
        Assert.Empty(partida.OpcionesDe(partida.JugadorEnTurno));
    }

    [Fact]
    public void Salida_solo_admite_la_ficha_de_salida()
    {
        var partida = new Partida(Apoyo.Configuracion(TipoDomino.Doble6, 3), 3, new RelojFalso());
        partida.Iniciar();
        var otra = partida.JugadorEnTurno.Mano.Fichas.First(f => f != new Ficha(6, 6));

        Assert.Throws<InvalidOperationException>(() => partida.JugarFicha(otra, Extremo.Izquierdo));
        Assert.Throws<InvalidOperationException>(() => partida.Pasar());
        Assert.Throws<InvalidOperationException>(() => partida.Iniciar());

        partida.JugarFicha(new Ficha(6, 6), Extremo.Izquierdo);
        Assert.Equal(EstadoPartida.EnCurso, partida.Estado);
    }

    [Fact]
    public void En_curso_no_admite_robar_ni_pasar_con_ficha_jugable()
    {
        var controlador = Apoyo.Controlador(5);
        var vista = Apoyo.AvanzarHasta(controlador, controlador.IniciarPartida(Apoyo.Configuracion()),
            v => v.Estado == EstadoPartida.EnCurso && v.Opciones.Count > 0)!;

        Assert.Throws<InvalidOperationException>(() => controlador.Robar());
        Assert.Throws<InvalidOperationException>(() => controlador.Pasar());
        var ajena = new Ficha(0, 0);
        if (!vista.ManoPropia.Contains(ajena))
            Assert.Throws<InvalidOperationException>(() => controlador.JugarFicha(ajena, Extremo.Izquierdo));
    }

    [Fact]
    public void Terminada_no_admite_acciones()
    {
        var controlador = Apoyo.Controlador(9);
        var vista = controlador.IniciarPartida(Apoyo.Configuracion(TipoDomino.Doble6, 1));
        Assert.Null(Apoyo.AvanzarHasta(controlador, vista, _ => false));

        Assert.Equal(EstadoPartida.Terminada, controlador.ObtenerVista().Estado);
        Assert.NotNull(controlador.ObtenerVista().Resultado);
        Assert.Throws<InvalidOperationException>(() => controlador.Pasar());
        Assert.Throws<InvalidOperationException>(() => controlador.DeshacerUltimaJugada());
    }

    [Fact]
    public void El_tiempo_agotado_hace_perder_el_turno_al_humano()
    {
        var reloj = new RelojFalso();
        var controlador = Apoyo.Controlador(11, reloj);
        var vista = Apoyo.AvanzarHasta(controlador,
            controlador.IniciarPartida(Apoyo.Configuracion(limite: LimiteTiempo.Segundos10)),
            v => v.Estado == EstadoPartida.EnCurso && v.Opciones.Count > 0)!;
        var fichasAntes = vista.ManoPropia.Count;
        Assert.Equal(10, vista.SegundosRestantes);

        reloj.Avanzar(11);
        vista = controlador.JugarFicha(vista.Opciones[0].Ficha, vista.Opciones[0].Extremo);

        Assert.Contains(vista.UltimasJugadas, j => j is JugadaTiempoAgotado && j.PosicionJugador == 0);
        Assert.True(vista.ManoPropia.Count >= fichasAntes, "La ficha elegida no debió colocarse.");
    }

    [Fact]
    public void En_la_salida_no_corre_el_tiempo()
    {
        var reloj = new RelojFalso();
        for (var semilla = 0; semilla < 50; semilla++)
        {
            var partida = new Partida(Apoyo.Configuracion(limite: LimiteTiempo.Segundos10), semilla, reloj);
            partida.Iniciar();
            reloj.Avanzar(60);

            Assert.False(partida.VerificarTiempo());
            Assert.Null(partida.ObtenerVistaPara(partida.Humano).SegundosRestantes);
        }
    }
}
