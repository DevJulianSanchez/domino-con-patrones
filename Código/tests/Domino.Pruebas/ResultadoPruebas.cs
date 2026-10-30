using Domino.Dominio;

namespace Domino.Pruebas;

/// <summary>RN-09: desempate del juego cerrado.</summary>
public class ResultadoPruebas
{
    private static Jugador Con(string alias, int posicion, params Ficha[] fichas)
    {
        var jugador = new JugadorHumano(alias, posicion);
        foreach (var ficha in fichas)
            jugador.Mano.Agregar(ficha);
        return jugador;
    }

    [Fact]
    public void Gana_la_menor_suma_de_puntos()
    {
        var resultado = Resultado.PorJuegoCerrado([Con("A", 0, new Ficha(5, 6)), Con("B", 1, new Ficha(1, 2), new Ficha(0, 3))]);

        Assert.Equal("B", resultado.Ganador!.Alias);
    }

    [Fact]
    public void En_empate_de_puntos_gana_quien_tenga_menos_fichas()
    {
        var resultado = Resultado.PorJuegoCerrado([Con("A", 0, new Ficha(1, 2), new Ficha(0, 3)), Con("B", 1, new Ficha(2, 4))]);

        Assert.Equal("B", resultado.Ganador!.Alias);
    }

    [Fact]
    public void Si_persiste_el_empate_no_hay_ganador()
    {
        var resultado = Resultado.PorJuegoCerrado([Con("A", 0, new Ficha(1, 5)), Con("B", 1, new Ficha(2, 4))]);

        Assert.True(resultado.EsEmpate);
    }
}
