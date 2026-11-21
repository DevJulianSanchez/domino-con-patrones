using Domino.Dominio;

namespace Domino.Pruebas;

/// <summary>Builder y Factory Method: armar la configuración y crear los jugadores.</summary>
public class CreacionPruebas
{
    [Fact]
    public void El_constructor_parte_de_los_valores_predeterminados_de_CU01()
    {
        var configuracion = new ConstructorConfiguracion().ConAlias("  Ana ").Construir();

        Assert.Equal("Ana", configuracion.Alias);
        Assert.Equal(TipoDomino.Doble6, configuracion.Tipo);
        Assert.Equal([NivelDificultad.Intermedio], configuracion.NivelesCpu);
        Assert.Equal(LimiteTiempo.SinLimite, configuracion.Limite);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UnAliasDemasiadoLargo")]
    public void El_alias_invalido_se_rechaza_en_su_propio_paso(string alias)
    {
        Assert.Throws<ArgumentException>(() => new ConstructorConfiguracion().ConAlias(alias));
    }

    [Fact]
    public void No_admite_una_cuarta_cpu()
    {
        var constructor = new ConstructorConfiguracion().ConAlias("Ana");
        foreach (var _ in ConstructorConfiguracion.CantidadesCpu)
            constructor.AgregarCpu(NivelDificultad.Avanzado);

        Assert.Throws<ArgumentException>(() => constructor.AgregarCpu(NivelDificultad.Avanzado));
        Assert.Equal(ConfiguracionPartida.MaximoCpu, constructor.CantidadCpu);
    }

    [Fact]
    public void Construir_sin_alias_falla()
    {
        Assert.Throws<InvalidOperationException>(() => new ConstructorConfiguracion().Construir());
    }

    [Fact]
    public void La_fabrica_crea_al_humano_primero_y_cada_cpu_con_la_estrategia_de_su_nivel()
    {
        var configuracion = new ConstructorConfiguracion().ConAlias("Ana")
            .AgregarCpu(NivelDificultad.Avanzado).AgregarCpu(NivelDificultad.Principiante).Construir();

        var jugadores = new FabricaJugadores(7).CrearJugadores(configuracion);

        Assert.IsType<JugadorHumano>(jugadores[0]);
        Assert.Equal([0, 1, 2], jugadores.Select(j => j.Posicion));
        Assert.Equal(NivelDificultad.Avanzado, Assert.IsType<JugadorCpu>(jugadores[1]).Nivel);
        Assert.Equal(NivelDificultad.Principiante, Assert.IsType<JugadorCpu>(jugadores[2]).Nivel);
    }

    [Theory]
    [InlineData(NivelDificultad.Principiante, typeof(EstrategiaPrincipiante))]
    [InlineData(NivelDificultad.Intermedio, typeof(EstrategiaIntermedia))]
    [InlineData(NivelDificultad.Avanzado, typeof(EstrategiaAvanzada))]
    public void El_metodo_de_fabricacion_elige_la_estrategia_concreta(NivelDificultad nivel, Type esperada)
    {
        var estrategia = new FabricaJugadores(1).CrearEstrategia(nivel, 1);

        Assert.IsType(esperada, estrategia);
        Assert.Equal(nivel, estrategia.Nivel);
    }

    [Fact]
    public void La_misma_semilla_produce_las_mismas_decisiones_del_principiante()
    {
        var opciones = Enumerable.Range(0, 6).Select(i => new OpcionJugada(new Ficha(i, 6), Extremo.Izquierdo)).ToList();
        var vista = Apoyo.Vista(opciones.Select(o => o.Ficha).ToList(), 6, 6);
        var a = new FabricaJugadores(42).CrearEstrategia(NivelDificultad.Principiante, 2);
        var b = new FabricaJugadores(42).CrearEstrategia(NivelDificultad.Principiante, 2);

        for (var i = 0; i < 20; i++)
            Assert.Equal(a.Elegir(opciones, vista), b.Elegir(opciones, vista));
    }
}
