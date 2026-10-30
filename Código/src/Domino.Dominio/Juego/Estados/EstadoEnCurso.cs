namespace Domino.Dominio;

/// <summary>
/// Juego en marcha: admite colocar, robar y pasar según las reglas (RN-06, RN-07), controla el
/// tiempo del turno (RN-04) y permite deshacer la última jugada del humano (RN-10).
/// </summary>
internal sealed class EstadoEnCurso : EstadoJuego
{
    public override EstadoPartida Valor => EstadoPartida.EnCurso;

    public override bool EnJuego => true;

    public override bool PermiteDeshacer => true;

    public override bool ControlaTiempo => true;

    public override IReadOnlyList<OpcionJugada> OpcionesDe(Partida partida, Jugador jugador) =>
        partida.OpcionesSobreMesa(jugador);

    public override void Procesar(Partida partida, Jugada jugada)
    {
        if (!partida.EsValida(jugada))
            throw new InvalidOperationException($"La jugada {jugada.Tipo} no es válida en este momento.");
        partida.Aplicar(jugada);
    }

    public override bool VerificarTiempo(Partida partida)
    {
        if (!partida.TiempoAgotado())
            return false;
        partida.Aplicar(partida.CrearJugadaTiempoAgotado());
        return true;
    }

    public override void Deshacer(Partida partida)
    {
        if (!partida.HayJugadaDe(partida.Humano))
            throw new InvalidOperationException("No hay una jugada propia para deshacer.");
        partida.RevertirHastaUltimaDe(partida.Humano);
    }
}
