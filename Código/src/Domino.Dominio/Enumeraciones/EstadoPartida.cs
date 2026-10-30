namespace Domino.Dominio;

/// <summary>Valor visible del estado de la partida; el comportamiento de cada estado está en EstadoJuego.</summary>
public enum EstadoPartida
{
    Preparada,
    Salida,
    EnCurso,
    Terminada
}
