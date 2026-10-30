namespace Domino.Dominio;

/// <summary>
/// Evalúa cada opción por el número de fichas propias que seguirían siendo jugables con los
/// extremos resultantes y desempata por puntos.
/// </summary>
public sealed class EstrategiaAvanzada : IEstrategiaCpu
{
    public NivelDificultad Nivel => NivelDificultad.Avanzado;

    public OpcionJugada Elegir(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        opciones
            .OrderByDescending(o => Evaluar(o, vista))
            .ThenByDescending(o => o.Ficha.Puntos)
            .First();

    private static int Evaluar(OpcionJugada opcion, VistaPartida vista)
    {
        var (izquierdo, derecho) = ExtremosTrasColocar(opcion, vista);
        return vista.ManoPropia
            .Where(f => f != opcion.Ficha)
            .Count(f => f.Contiene(izquierdo) || f.Contiene(derecho));
    }

    private static (int Izquierdo, int Derecho) ExtremosTrasColocar(OpcionJugada opcion, VistaPartida vista)
    {
        if (vista.ExtremoIzquierdo is not { } izquierdo || vista.ExtremoDerecho is not { } derecho)
            return (opcion.Ficha.ValorA, opcion.Ficha.ValorB);

        return opcion.Extremo == Extremo.Izquierdo
            ? (opcion.Ficha.OtroValor(izquierdo), derecho)
            : (izquierdo, opcion.Ficha.OtroValor(derecho));
    }
}
