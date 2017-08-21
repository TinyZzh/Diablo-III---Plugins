using System;
using System.Diagnostics;

namespace StatsTracker.Util
{
    internal class PerformanceLogger : IDisposable
    {
        private Stopwatch Stopwatch { get; set; }
        private readonly string _message;
        internal PerformanceLogger(string message)
        {
            _message = message;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public void Dispose()
        {
            var elapsed = Stopwatch.Elapsed.TotalMilliseconds;
            elapsed = Math.Round(elapsed * 10000) / 10000;
            Logger.Debug(string.Format("{0} took {1} ms.", _message, elapsed));
            Stopwatch.Stop();
        }
    }
}
