using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StatsTracker.Util;

namespace StatsTracker.Trackers
{
    internal class DpsTracker
    {
        private static readonly ConcurrentDictionary<long, float> DamageTable =
            new ConcurrentDictionary<long, float>();

        public void AddDamage(long currentSecond, float damageToAdd)
        {
            DamageTable.AddOrUpdate(currentSecond, damageToAdd, (cs, currentDamage) => currentDamage + damageToAdd);
        }

        public static List<float> GetReport()
        {
            return DamageTable.Values.ToList();
        }
        public static List<float> GetLastSecondsReport(long seconds)
        {
            return DamageTable.Where(d => d.Key >= TimeProvider.CurrentSecond - seconds).Select(d => d.Value).ToList();
        }

        public static float GetMaxDps()
        {
            return DamageTable.Count>0? DamageTable.Values.Max():0;
        }

        public static void Reset()
        {
            DamageTable.Clear();
        }

    }

    internal class CurrentDpsTracker
    {
        private static readonly ConcurrentDictionary<long, float> DamageTable =
            new ConcurrentDictionary<long, float>();


        public void AddDamage(long currentMillisecond, float damageToAdd)
        {
            DamageTable.AddOrUpdate(currentMillisecond, damageToAdd, (cs, currentDamage) => currentDamage + damageToAdd);
        }

        public static float GetCurrentDps()
        {
            if (DamageTable.Count == 0) return 0;
            return DamageTable.Where(d => d.Key >= TimeProvider.CurrentMillisecond - 1000).Sum(d => d.Value);
        }

        public static void Reset()
        {
            DamageTable.Clear();
        }


        #region Cleanup
        private static readonly TimeSpan CleanupTimeSpan = new TimeSpan(0, 0, 0, 15);
        private static readonly Timer CleanupTimer = new Timer(Cleanup, null, new TimeSpan(0, 0, 0, 15), Timeout.InfiniteTimeSpan);

        private static void Cleanup(object state)
        {
            CleanupJob();
            CleanupTimer.Change(CleanupTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private static void CleanupJob()
        {
            var entriesToRemove = DamageTable.Keys.Where(k => k < TimeProvider.CurrentMillisecond - 1000);
            foreach (var l in entriesToRemove)
            {
                float toRemove;
                DamageTable.TryRemove(l, out toRemove);
            }
        }
        #endregion

    }


}