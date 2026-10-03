namespace Domino.Dominio;

/// <summary>Fuente de la hora actual; permite usar un reloj controlado en la simulación.</summary>
public interface IReloj
{
    DateTime Ahora { get; }
}
