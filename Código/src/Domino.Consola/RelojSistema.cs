using Domino.Dominio;

namespace Domino.Consola;

/// <summary>Adapter: expone el reloj de la plataforma (DateTime.Now) con la interfaz IReloj del dominio.</summary>
public sealed class RelojSistema : IReloj
{
    public DateTime Ahora => DateTime.Now;
}
