namespace Domino.Dominio;

/// <summary>Elige al azar una de las opciones válidas.</summary>
public sealed class JugadorCpuPrincipiante : JugadorCpu
{
    private readonly Random aleatorio;

    public JugadorCpuPrincipiante(string alias, int posicion, Random aleatorio) : base(alias, posicion)
    {
        this.aleatorio = aleatorio;
    }

    public override NivelDificultad Nivel => NivelDificultad.Principiante;

    public override OpcionJugada DecidirJugada(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        throw new NotImplementedException("Pendiente: elegir al azar con 'aleatorio'.");
}
