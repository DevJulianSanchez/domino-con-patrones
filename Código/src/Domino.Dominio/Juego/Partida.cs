namespace Domino.Dominio;

/// <summary>
/// Aplica las reglas del juego y coordina la mesa, los jugadores, el reparto y el historial.
/// Es el contexto de State (delega en su estado actual las acciones que dependen de él) y el
/// invocador de Command (ejecuta, registra y revierte las jugadas).
/// </summary>
public sealed class Partida
{
    /// <summary>Cantidad de jugadas recientes que incluye la vista.</summary>
    private const int JugadasRecientes = 12;

    private readonly ConfiguracionPartida configuracion;
    private readonly DefinicionTipoDomino definicion;
    private readonly int semilla;
    private readonly IReloj reloj;
    private readonly List<Jugador> jugadores = [];
    private readonly Mesa mesa = new();
    private readonly HistorialJugadas historial = new();
    private FichasParaRobar fichasParaRobar = new([]);
    private EstadoJuego estado = new EstadoPreparada();
    private int turnoActual;
    private DateTime inicioTurno;
    private Ficha? fichaSalida;
    private Resultado? resultado;

    public Partida(ConfiguracionPartida configuracion, int semilla, IReloj reloj)
    {
        configuracion.Validar();
        this.configuracion = configuracion;
        this.semilla = semilla;
        this.reloj = reloj;
        definicion = DefinicionTipoDomino.De(configuracion.Tipo);
        CrearJugadores();
    }

    public EstadoPartida Estado => estado.Valor;

    public Jugador JugadorEnTurno => jugadores[turnoActual];

    public JugadorHumano Humano => (JugadorHumano)jugadores[0];

    /// <summary>Reparte las fichas y determina quién abre y con qué ficha (CU-02, RN-05).</summary>
    public void Iniciar() => estado.Iniciar(this);

    public void JugarFicha(Ficha ficha, Extremo extremo) =>
        estado.Procesar(this, new JugadaColocarFicha(SiguienteOrden, turnoActual, ficha, extremo));

    public void Robar() => estado.Procesar(this, new JugadaRobar(SiguienteOrden, turnoActual));

    public void Pasar() => estado.Procesar(this, new JugadaPasar(SiguienteOrden, turnoActual));

    /// <summary>Juega los turnos de las CPU hasta que el turno sea del humano o la partida termine.</summary>
    public void ResolverTurnosCpu()
    {
        while (estado.EnJuego && JugadorEnTurno is JugadorCpu cpu)
        {
            var opciones = OpcionesDe(cpu);
            if (opciones.Count > 0)
            {
                var elegida = cpu.DecidirJugada(opciones, ObtenerVistaPara(cpu));
                JugarFicha(elegida.Ficha, elegida.Extremo);
            }
            else if (!fichasParaRobar.EstaVacia)
                Robar();
            else
                Pasar();
        }
    }

    /// <summary>Si el jugador en turno agotó su tiempo, pierde el turno (RN-04). Indica si ocurrió.</summary>
    public bool VerificarTiempo() => estado.VerificarTiempo(this);

    /// <summary>Revierte la última jugada del humano y las de las CPU posteriores (RN-10).</summary>
    public void DeshacerUltimaJugadaHumano() => estado.Deshacer(this);

    public IReadOnlyList<OpcionJugada> OpcionesDe(Jugador jugador) =>
        jugador == JugadorEnTurno ? estado.OpcionesDe(this, jugador) : [];

    public VistaPartida ObtenerVistaPara(Jugador jugador)
    {
        var esTurnoPropio = estado.EnJuego && jugador == JugadorEnTurno;
        var opciones = esTurnoPropio ? OpcionesDe(jugador) : [];
        var sinOpciones = esTurnoPropio && opciones.Count == 0;

        return new VistaPartida
        {
            Estado = estado.Valor,
            Jugadores = jugadores.Select(j => j.Alias).ToList(),
            FichasPorJugador = jugadores.Select(j => j.Mano.Cantidad).ToList(),
            PosicionPropia = jugador.Posicion,
            PosicionEnTurno = turnoActual,
            EsTurnoPropio = esTurnoPropio,
            ManoPropia = jugador.Mano.Fichas.ToList(),
            Mesa = mesa.Colocadas,
            ExtremoIzquierdo = mesa.EstaVacia ? null : mesa.ValorExtremo(Extremo.Izquierdo),
            ExtremoDerecho = mesa.EstaVacia ? null : mesa.ValorExtremo(Extremo.Derecho),
            FichasParaRobar = fichasParaRobar.Cantidad,
            Opciones = opciones,
            PuedeRobar = sinOpciones && !fichasParaRobar.EstaVacia,
            PuedePasar = sinOpciones && fichasParaRobar.EstaVacia,
            PuedeDeshacer = estado.PermiteDeshacer && jugador == Humano && historial.HayJugadaDe(jugador.Posicion),
            SegundosRestantes = esTurnoPropio && estado.ControlaTiempo && jugador.AplicaLimiteTiempo
                                && configuracion.SegundosPorTurno is { } limite
                ? Math.Max(0, limite - (int)(reloj.Ahora - inicioTurno).TotalSeconds)
                : null,
            UltimasJugadas = historial.Ultimas(JugadasRecientes),
            Resultado = resultado
        };
    }

    public PartidaGuardada CrearGuardado() => new(configuracion, semilla, historial.Todas.ToList());

    /// <summary>
    /// Crea una partida con la misma configuración y semilla, la inicia (mismo reparto) y vuelve a
    /// procesar las jugadas guardadas en orden, sin consultar a las CPU (RN-11).
    /// </summary>
    public static Partida Restaurar(PartidaGuardada guardado, IReloj reloj)
    {
        var partida = new Partida(guardado.Configuracion, guardado.Semilla, reloj);
        partida.Iniciar();
        foreach (var jugada in guardado.Jugadas)
            partida.estado.Procesar(partida, jugada);
        partida.IniciarTurno();
        return partida;
    }

    // ---------- Operaciones que usan los estados (State) ----------

    internal Ficha? FichaSalida => fichaSalida;

    internal void CambiarEstado(EstadoJuego nuevo) => estado = nuevo;

    internal void Repartir()
    {
        fichasParaRobar = new Repartidor(semilla).Repartir(jugadores, definicion);
        DeterminarSalida();
        IniciarTurno();
    }

    internal IReadOnlyList<OpcionJugada> OpcionesSobreMesa(Jugador jugador) =>
        jugador.Mano.Fichas
            .SelectMany(f => new[] { new OpcionJugada(f, Extremo.Izquierdo), new OpcionJugada(f, Extremo.Derecho) })
            .Where(o => mesa.PuedeColocar(o.Ficha, o.Extremo))
            .ToList();

    /// <summary>La jugada corresponde al jugador en turno y cumple sus propias condiciones.</summary>
    internal bool EsValida(Jugada jugada) =>
        jugada.PosicionJugador == turnoActual && jugada.EsValida(mesa, JugadorEnTurno.Mano, fichasParaRobar);

    /// <summary>Ejecuta la jugada (Command), la registra, verifica el fin y, si corresponde, avanza el turno.</summary>
    internal void Aplicar(Jugada jugada)
    {
        jugada.Ejecutar(mesa, jugadores[jugada.PosicionJugador].Mano, fichasParaRobar);
        historial.Registrar(jugada);
        if (VerificarFin(jugadores[jugada.PosicionJugador]))
            return;
        if (jugada.TerminaTurno)
            AvanzarTurno();
    }

    internal bool TiempoAgotado() =>
        JugadorEnTurno.AplicaLimiteTiempo
        && configuracion.SegundosPorTurno is { } limite
        && (reloj.Ahora - inicioTurno).TotalSeconds >= limite;

    internal Jugada CrearJugadaTiempoAgotado() => new JugadaTiempoAgotado(SiguienteOrden, turnoActual);

    internal bool HayJugadaDe(Jugador jugador) => historial.HayJugadaDe(jugador.Posicion);

    /// <summary>Deshace, de la más reciente a la más antigua, las jugadas desde la última del jugador.</summary>
    internal void RevertirHastaUltimaDe(Jugador jugador)
    {
        foreach (var jugada in historial.ExtraerHastaUltimaDe(jugador.Posicion))
            jugada.Deshacer(mesa, jugadores[jugada.PosicionJugador].Mano, fichasParaRobar);

        turnoActual = jugador.Posicion;
        IniciarTurno();
        CambiarEstado(mesa.EstaVacia ? new EstadoSalida() : new EstadoEnCurso());
    }

    // ---------- Apoyo interno ----------

    private int SiguienteOrden => historial.Cantidad + 1;

    /// <summary>El humano ocupa la posición 0 y las CPU las siguientes, con la estrategia de su nivel (RN-02).</summary>
    private void CrearJugadores()
    {
        jugadores.Add(new JugadorHumano(configuracion.Alias.Trim(), 0));
        for (var i = 0; i < configuracion.NivelesCpu.Count; i++)
        {
            var posicion = i + 1;
            var nivel = configuracion.NivelesCpu[i];
            IEstrategiaCpu estrategia = nivel switch
            {
                NivelDificultad.Principiante => new EstrategiaPrincipiante(new Random(semilla + posicion)),
                NivelDificultad.Intermedio => new EstrategiaIntermedia(),
                NivelDificultad.Avanzado => new EstrategiaAvanzada(),
                _ => throw new ArgumentOutOfRangeException(nameof(configuracion), nivel, "Nivel de dificultad no soportado.")
            };
            jugadores.Add(new JugadorCpu($"CPU {posicion} ({nivel})", posicion, estrategia));
        }
    }

    /// <summary>
    /// RN-05: abre quien tenga el doble más alto entre todos los jugadores. Sin dobles, abre la
    /// ficha de mayor puntaje y, en empate, la de número individual más alto.
    /// </summary>
    private void DeterminarSalida()
    {
        var conDoble = jugadores
            .Select(j => (Jugador: j, Ficha: j.Mano.MayorDoble()))
            .Where(x => x.Ficha is not null)
            .OrderByDescending(x => x.Ficha!.ValorA)
            .FirstOrDefault();
        if (conDoble.Jugador is not null)
        {
            turnoActual = conDoble.Jugador.Posicion;
            fichaSalida = conDoble.Ficha;
            return;
        }

        var mayor = jugadores
            .Select(j => (Jugador: j, Ficha: j.Mano.FichaDeMayorPuntaje()!))
            .OrderByDescending(x => x.Ficha.Puntos)
            .ThenByDescending(x => x.Ficha.ValorMayor)
            .First();
        turnoActual = mayor.Jugador.Posicion;
        fichaSalida = mayor.Ficha;
    }

    private void AvanzarTurno()
    {
        turnoActual = (turnoActual + 1) % jugadores.Count;
        IniciarTurno();
    }

    private void IniciarTurno() => inicioTurno = reloj.Ahora;

    /// <summary>Dominó si el jugador quedó sin fichas (RN-08); juego cerrado si nadie puede jugar ni robar (RN-09).</summary>
    private bool VerificarFin(Jugador ultimo)
    {
        if (ultimo.Mano.EstaVacia)
            resultado = Resultado.PorDomino(ultimo, jugadores);
        else if (EstaCerrado())
            resultado = Resultado.PorJuegoCerrado(jugadores);
        else
            return false;

        CambiarEstado(new EstadoTerminada());
        return true;
    }

    private bool EstaCerrado() =>
        fichasParaRobar.EstaVacia && jugadores.TrueForAll(j => !mesa.AdmiteAlgunaDe(j.Mano.Fichas));
}
