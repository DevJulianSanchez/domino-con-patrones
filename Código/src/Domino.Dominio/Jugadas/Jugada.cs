namespace Domino.Dominio;

/// <summary>
/// Command: acción del juego como objeto. Sabe si es válida, cómo ejecutarse y cómo deshacerse
/// sobre la mesa, la mano del jugador y las fichas para robar, de modo que el historial puede
/// revertirla (RN-10) y la partida guardada puede volver a ejecutarla (RN-11). El jugador se
/// identifica por su posición. Solo el dominio puede ejecutarla o deshacerla.
/// </summary>
public abstract class Jugada
{
    protected Jugada(int orden, int posicionJugador)
    {
        Orden = orden;
        PosicionJugador = posicionJugador;
    }

    public int Orden { get; }

    public int PosicionJugador { get; }

    public abstract TipoJugada Tipo { get; }

    /// <summary>Indica si, al ejecutarse, el turno pasa al siguiente jugador.</summary>
    internal abstract bool TerminaTurno { get; }

    internal abstract bool EsValida(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar);

    internal abstract void Ejecutar(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar);

    internal abstract void Deshacer(Mesa mesa, Mano mano, FichasParaRobar fichasParaRobar);
}
