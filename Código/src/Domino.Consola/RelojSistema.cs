using Domino.Dominio;

namespace Domino.Consola;

public sealed class RelojSistema : IReloj
{
    public DateTime Ahora => DateTime.Now;
}
