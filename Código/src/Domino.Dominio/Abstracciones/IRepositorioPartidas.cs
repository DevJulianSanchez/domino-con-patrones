namespace Domino.Dominio;

/// <summary>Medio de almacenamiento de las partidas guardadas.</summary>
public interface IRepositorioPartidas
{
    void Guardar(string nombre, PartidaGuardada partida);

    PartidaGuardada Cargar(string nombre);

    IReadOnlyList<string> ListarNombres();
}
