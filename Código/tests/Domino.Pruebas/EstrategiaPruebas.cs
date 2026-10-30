using Domino.Dominio;

namespace Domino.Pruebas;

/// <summary>Strategy: cada nivel elige con su propio criterio entre las mismas opciones.</summary>
public class EstrategiaPruebas
{
    [Fact]
    public void Intermedia_elige_la_ficha_de_mas_puntos_y_prefiere_el_doble_en_empate()
    {
        var opciones = new List<OpcionJugada>
        {
            new(new Ficha(2, 6), Extremo.Izquierdo),
            new(new Ficha(4, 4), Extremo.Derecho),
            new(new Ficha(1, 2), Extremo.Izquierdo)
        };
        var vista = Apoyo.Vista(opciones.Select(o => o.Ficha).ToList(), 2, 4);

        Assert.Equal(new Ficha(4, 4), new EstrategiaIntermedia().Elegir(opciones, vista).Ficha);
    }

    [Fact]
    public void Avanzada_elige_la_opcion_que_deja_mas_fichas_jugables()
    {
        // Mesa con extremos 3 y 5. Colocar [3|6] deja 6 y 5; colocar [5|1] deja 3 y 1.
        // La mano tiene dos fichas con 6 y ninguna con 1 ni 3, así que conviene [3|6].
        var mano = new List<Ficha> { new(3, 6), new(1, 5), new(6, 6), new(2, 6) };
        var opciones = new List<OpcionJugada>
        {
            new(new Ficha(1, 5), Extremo.Derecho),
            new(new Ficha(3, 6), Extremo.Izquierdo)
        };

        var elegida = new EstrategiaAvanzada().Elegir(opciones, Apoyo.Vista(mano, 3, 5));

        Assert.Equal(new Ficha(3, 6), elegida.Ficha);
    }

    [Fact]
    public void Principiante_elige_una_opcion_valida()
    {
        var opciones = new List<OpcionJugada> { new(new Ficha(0, 1), Extremo.Izquierdo), new(new Ficha(1, 1), Extremo.Derecho) };
        var estrategia = new EstrategiaPrincipiante(new Random(1));

        for (var i = 0; i < 20; i++)
            Assert.Contains(estrategia.Elegir(opciones, Apoyo.Vista([], 1, 1)), opciones);
    }

    [Fact]
    public void El_nivel_de_la_cpu_lo_define_su_estrategia()
    {
        var cpu = new JugadorCpu("CPU", 1, new EstrategiaAvanzada());

        Assert.Equal(NivelDificultad.Avanzado, cpu.Nivel);
        Assert.True(cpu.EsAutomatico);
        Assert.False(cpu.AplicaLimiteTiempo);
    }
}
