namespace Domino.Dominio;

/// <summary>Elige al azar una de las opciones válidas.</summary>
public sealed class EstrategiaPrincipiante : IEstrategiaCpu
{
    private readonly Random aleatorio;

    public EstrategiaPrincipiante(Random aleatorio)
    {
        this.aleatorio = aleatorio;
    }

    public NivelDificultad Nivel => NivelDificultad.Principiante;

    public OpcionJugada Elegir(IReadOnlyList<OpcionJugada> opciones, VistaPartida vista) =>
        opciones[aleatorio.Next(opciones.Count)];
}
