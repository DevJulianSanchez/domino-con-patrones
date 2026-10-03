using Domino.Dominio;

namespace Domino.Simulacion;

/// <summary>Reloj controlado: el tiempo solo avanza cuando el simulador lo indica.</summary>
public sealed class RelojSimulado : IReloj
{
    private DateTime ahora;

    public RelojSimulado(DateTime inicio)
    {
        ahora = inicio;
    }

    public DateTime Ahora => ahora;

    public void Avanzar(int segundos) => ahora = ahora.AddSeconds(segundos);
}
