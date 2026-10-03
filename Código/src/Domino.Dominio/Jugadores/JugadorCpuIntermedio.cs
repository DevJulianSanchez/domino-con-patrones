namespace Domino.Dominio;

/// <summary>Elige la opción cuya ficha suma más puntos; en empate prefiere los dobles.</summary>
public sealed class JugadorCpuIntermedio(string alias, int posicion) : JugadorCpu(alias, posicion)
{
    public override NivelDificultad Nivel => NivelDificultad.Intermedio;

    public override OpcionJugada DecidirJugada(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        throw new NotImplementedException("Pendiente: elegir la ficha de mayor puntaje.");
}
