using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsTracker.Util
{
    internal static class TimeProvider
    {
        private static readonly Stopwatch StopwatchInstance = new Stopwatch();
        static TimeProvider()
        {
            StopwatchInstance.Start();
        }

        internal static long CurrentSecond
        {
            get { return StopwatchInstance.ElapsedMilliseconds/1000; }
        }

        internal static long CurrentMillisecond
        {
            get { return StopwatchInstance.ElapsedMilliseconds; }
        }
    }
}
