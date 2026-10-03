namespace Domino.Dominio;

/// <summary>
/// Evalúa cada opción por el número de fichas propias que seguirían siendo jugables con los
/// extremos resultantes y desempata por puntos.
/// </summary>
public sealed class JugadorCpuAvanzado(string alias, int posicion) : JugadorCpu(alias, posicion)
{
    public override NivelDificultad Nivel => NivelDificultad.Avanzado;

    public override OpcionJugada DecidirJugada(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        throw new NotImplementedException("Pendiente: elegir la opción con mayor Evaluar(opcion, vista).");

    private int Evaluar(OpcionJugada opcion, VistaPartida vista) =>
        throw new NotImplementedException("Pendiente: contar las fichas propias jugables tras colocar la opción.");
}
