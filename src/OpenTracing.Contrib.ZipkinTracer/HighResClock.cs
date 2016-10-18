using System;
using System.Diagnostics;

namespace OpenTracing.Contrib.ZipkinTracer
{
    /// <summary>
    /// The value of the system clock that DateTime.Now reads is only updated every 15 ms or so (or 10 ms on some systems).
    /// WARNING: The Stopwatch runs out of sync (by as much as half a second per hour)! This means,
    /// an instance of this class must be short-lived!
    /// (It could be re-synced (see SO link) but this would also lead to wrong calculations if it happens
    /// to be re-synced during a running span.
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/1416139/how-to-get-timestamp-of-tick-precision-in-net-c
    /// </remarks>
    public class HighResClock
    {
        private DateTime _startTime = DateTime.UtcNow;
        private Stopwatch _stopwatch = Stopwatch.StartNew();

        public DateTime UtcNow =>_startTime.AddTicks(_stopwatch.Elapsed.Ticks);
    }
}