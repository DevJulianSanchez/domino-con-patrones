using Domino.Consola;
using Domino.Dominio;
using Domino.Persistencia;

// Punto de entrada: arma las dependencias y entrega el control a la interfaz.
var carpeta = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DominoPatrones", "partidas");
var controlador = new ControladorJuego(new RelojSistema(), new RepositorioPartidasJson(carpeta), new Random());
new InterfazConsola(controlador).Ejecutar();
