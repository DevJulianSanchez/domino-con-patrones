namespace Domino.Dominio;

/// <summary>Aplica las reglas del juego y coordina la mesa, los jugadores, el reparto y el historial.</summary>
public sealed class Partida
{
    private readonly ConfiguracionPartida configuracion;
    private readonly DefinicionTipoDomino definicion;
    private readonly int semilla;
    private readonly IReloj reloj;
    private readonly List<Jugador> jugadores = [];
    private readonly Mesa mesa = new();
    private readonly HistorialJugadas historial = new();
    private FichasParaRobar? fichasParaRobar;
    private EstadoPartida estado = EstadoPartida.Preparada;
    private int turnoActual;
    private DateTime inicioTurno;
    private Ficha? fichaSalida;
    private Resultado? resultado = null;

    public Partida(ConfiguracionPartida configuracion, int semilla, IReloj reloj)
    {
        configuracion.Validar();
        this.configuracion = configuracion;
        this.semilla = semilla;
        this.reloj = reloj;
        definicion = DefinicionTipoDomino.De(configuracion.Tipo);
        CrearJugadores();
    }

    public EstadoPartida Estado => estado;

    public Jugador JugadorEnTurno => jugadores[turnoActual];

    public JugadorHumano Humano => (JugadorHumano)jugadores[0];

    /// <summary>Reparte las fichas y determina quién abre y con qué ficha (CU-02, RN-05).</summary>
    public void Iniciar()
    {
        if (estado != EstadoPartida.Preparada)
            throw new InvalidOperationException("La partida ya fue iniciada.");

        fichasParaRobar = new Repartidor(semilla).Repartir(jugadores, definicion);
        DeterminarSalida();
        estado = EstadoPartida.Salida;
        inicioTurno = reloj.Ahora;
    }

    public void JugarFicha(Ficha ficha, Extremo extremo) => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public void Robar() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public void Pasar() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public void ResolverTurnosCpu() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public bool VerificarTiempo() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public void DeshacerUltimaJugadaHumano() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    public IReadOnlyList<OpcionJugada> OpcionesDe(Jugador jugador)
    {
        if (jugador != JugadorEnTurno)
            return [];

        if (estado == EstadoPartida.Salida)
            return fichaSalida is null ? [] : [new OpcionJugada(fichaSalida, Extremo.Izquierdo)];

        if (estado != EstadoPartida.EnCurso)
            return [];

        return jugador.Mano.Fichas
            .SelectMany(f => new[] { new OpcionJugada(f, Extremo.Izquierdo), new OpcionJugada(f, Extremo.Derecho) })
            .Where(o => mesa.PuedeColocar(o.Ficha, o.Extremo))
            .ToList();
    }

    public VistaPartida ObtenerVistaPara(Jugador jugador)
    {
        var enJuego = estado is EstadoPartida.Salida or EstadoPartida.EnCurso;
        var esTurnoPropio = enJuego && jugador == JugadorEnTurno;
        var opciones = esTurnoPropio ? OpcionesDe(jugador) : [];
        var paraRobar = fichasParaRobar?.Cantidad ?? 0;
        var sinOpciones = esTurnoPropio && estado == EstadoPartida.EnCurso && opciones.Count == 0;

        return new VistaPartida
        {
            Estado = estado,
            Jugadores = jugadores.Select(j => j.Alias).ToList(),
            FichasPorJugador = jugadores.Select(j => j.Mano.Cantidad).ToList(),
            PosicionPropia = jugador.Posicion,
            PosicionEnTurno = turnoActual,
            EsTurnoPropio = esTurnoPropio,
            ManoPropia = jugador.Mano.Fichas.ToList(),
            Mesa = mesa.Colocadas,
            ExtremoIzquierdo = mesa.EstaVacia ? null : mesa.ValorExtremo(Extremo.Izquierdo),
            ExtremoDerecho = mesa.EstaVacia ? null : mesa.ValorExtremo(Extremo.Derecho),
            FichasParaRobar = paraRobar,
            Opciones = opciones,
            PuedeRobar = sinOpciones && paraRobar > 0,
            PuedePasar = sinOpciones && paraRobar == 0,
            PuedeDeshacer = estado == EstadoPartida.EnCurso && jugador == Humano && historial.HayJugadaDe(jugador.Posicion),
            SegundosRestantes = esTurnoPropio && jugador.AplicaLimiteTiempo && configuracion.SegundosPorTurno is { } limite
                ? Math.Max(0, limite - (int)(reloj.Ahora - inicioTurno).TotalSeconds)
                : null,
            UltimasJugadas = historial.Ultimas(jugadores.Count),
            Resultado = resultado
        };
    }

    public PartidaGuardada CrearGuardado() => new(configuracion, semilla, historial.Todas.ToList());

    public static Partida Restaurar(PartidaGuardada guardado, IReloj reloj) =>
        throw new NotImplementedException("Pendiente: iniciar con la misma semilla y aplicar las jugadas guardadas.");

    /// <summary>El humano ocupa la posición 0 y las CPU las siguientes, con el nivel elegido (RN-02).</summary>
    private void CrearJugadores()
    {
        jugadores.Add(new JugadorHumano(configuracion.Alias.Trim(), 0));
        for (var i = 0; i < configuracion.NivelesCpu.Count; i++)
        {
            var posicion = i + 1;
            var nivel = configuracion.NivelesCpu[i];
            var alias = $"CPU {posicion} ({nivel})";
            jugadores.Add(nivel switch
            {
                NivelDificultad.Principiante => new JugadorCpuPrincipiante(alias, posicion, new Random(semilla + posicion)),
                NivelDificultad.Intermedio => new JugadorCpuIntermedio(alias, posicion),
                NivelDificultad.Avanzado => new JugadorCpuAvanzado(alias, posicion),
                _ => throw new ArgumentOutOfRangeException(nameof(configuracion), nivel, "Nivel de dificultad no soportado.")
            });
        }
    }

    /// <summary>
    /// RN-05: abre quien tenga el doble más alto, buscando desde el doble máximo hasta el doble 0.
    /// Sin dobles, abre la ficha de mayor puntaje y, en empate, la de número individual más alto.
    /// </summary>
    private void DeterminarSalida()
    {
        for (var valor = definicion.ValorMaximo; valor >= 0; valor--)
        {
            var doble = new Ficha(valor, valor);
            var duenio = jugadores.Find(j => j.Mano.Contiene(doble));
            if (duenio is not null)
            {
                turnoActual = duenio.Posicion;
                fichaSalida = doble;
                return;
            }
        }

        var mayor = jugadores
            .Select(j => (Jugador: j, Ficha: j.Mano.FichaDeMayorPuntaje()!))
            .OrderByDescending(x => x.Ficha.Puntos)
            .ThenByDescending(x => x.Ficha.ValorMayor)
            .First();
        turnoActual = mayor.Jugador.Posicion;
        fichaSalida = mayor.Ficha;
    }

    private void Aplicar(Jugada jugada) => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    private void Revertir(Jugada jugada) => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    private void AvanzarTurno() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    private void VerificarFin() => throw new NotImplementedException("Pendiente: ciclo de turnos.");

    private bool EstaCerrado() => throw new NotImplementedException("Pendiente: ciclo de turnos.");
}
