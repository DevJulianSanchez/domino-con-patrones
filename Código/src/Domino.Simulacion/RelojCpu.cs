using System.Runtime.InteropServices;

namespace Domino.Simulacion;

/// <summary>
/// Tiempo de CPU consumido por el hilo actual, con resolución de nanosegundos, mediante
/// clock_gettime(CLOCK_THREAD_CPUTIME_ID). En Windows usa el tiempo de CPU del proceso.
/// </summary>
internal static partial class RelojCpu
{
    private const int RelojHiloMac = 16;
    private const int RelojHiloLinux = 3;

    [StructLayout(LayoutKind.Sequential)]
    private struct Instante
    {
        public long Segundos;
        public long Nanosegundos;
    }

    [LibraryImport("libc", EntryPoint = "clock_gettime")]
    private static partial int ClockGettime(int reloj, out Instante instante);

    private static readonly int? Reloj =
        OperatingSystem.IsMacOS() ? RelojHiloMac : OperatingSystem.IsLinux() ? RelojHiloLinux : null;

    public static TimeSpan HiloActual()
    {
        if (Reloj is { } reloj && ClockGettime(reloj, out var t) == 0)
            return TimeSpan.FromTicks(t.Segundos * TimeSpan.TicksPerSecond + t.Nanosegundos / 100);
        return Environment.CpuUsage.TotalTime;
    }
}
