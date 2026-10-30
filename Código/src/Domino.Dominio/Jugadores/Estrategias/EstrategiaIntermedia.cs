namespace Domino.Dominio;

/// <summary>Elige la opción cuya ficha suma más puntos, para descargar la mano; en empate prefiere los dobles.</summary>
public sealed class EstrategiaIntermedia : IEstrategiaCpu
{
    public NivelDificultad Nivel => NivelDificultad.Intermedio;

    public OpcionJugada Elegir(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        opciones
            .OrderByDescending(o => o.Ficha.Puntos)
            .ThenByDescending(o => o.Ficha.EsDoble)
            .First();
}
