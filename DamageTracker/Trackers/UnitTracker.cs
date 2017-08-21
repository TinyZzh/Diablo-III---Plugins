using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using StatsTracker.UI;
using StatsTracker.Util;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace StatsTracker.Trackers
{
    internal class UnitTracker
    {

        private static readonly ConcurrentDictionary<int, Lazy<TrackedUnit>> SomeClasses =
            new ConcurrentDictionary<int, Lazy<TrackedUnit>>();

        private readonly TrackedUnit _privateClass;

        public UnitTracker(DiaUnit unit)
        {
           _privateClass = SomeClasses.GetOrAdd(unit.ACDId, (key) => new Lazy<TrackedUnit>(() => new TrackedUnit(unit))).Value;
        }

        public float GetAndSetHitpointsCurrent(float currentHitPoints)
        {
            var retVal = _privateClass.HitpointsCurrent;
            _privateClass.HitpointsCurrent = currentHitPoints;
            return retVal;
        }

        internal static void TrackUnits()
        {
            var currentSecond = TimeProvider.CurrentSecond;
            var currentMillisecond = TimeProvider.CurrentMillisecond;
            var dpsTracker = new DpsTracker();
            var currentDpsTracker = new CurrentDpsTracker();
            var allObjects = ZetaDia.Actors.GetActorsOfType<DiaUnit>().ToList();
            var mobsAroundMe = allObjects.Where(u => u != null && u.CommonData != null && u.IsValid && u.CommonData.IsValid && u.IsHostile).ToList();
            //Logger.Debug(string.Format("{0} mobs around me. {1} of them are dead. Their total hitpoints are {2:N}",
            //    mobsAroundMe.Count,
            //    mobsAroundMe.Count(m => !m.IsAlive),
            //    mobsAroundMe.Sum(m => m.HitpointsCurrent)
            //));

            foreach (var diaUnit in mobsAroundMe)
            {
                if (diaUnit.IsValid)
                {
                    var unitTracker = new UnitTracker(diaUnit);
                    var damageDoneToTarget = unitTracker.GetAndSetHitpointsCurrent(diaUnit.HitpointsCurrent) -
                                             diaUnit.HitpointsCurrent;
                    if (damageDoneToTarget > 1)
                    {
                        dpsTracker.AddDamage(currentSecond, damageDoneToTarget);
                        currentDpsTracker.AddDamage(currentMillisecond, damageDoneToTarget);
                        //Logger.Verbose("Damage to " + diaUnit.Name + "(" + diaUnit.ACDGuid + "): " + damageDoneToTarget);
                    }
                }
            }
            var mobIdsAroundMe = new HashSet<int>(mobsAroundMe.Select(m => m.ACDId));
            var trackedUnits = SomeClasses.Keys.ToList();
            foreach (var trackedUnit in trackedUnits.Where(trackedUnit => !mobIdsAroundMe.Contains(trackedUnit)))
            {
                Lazy<TrackedUnit> removedUnit;
                SomeClasses.TryRemove(trackedUnit, out removedUnit);
            }

        }

        private class TrackedUnit
        {
            public TrackedUnit(DiaUnit unit)
            {
                UnitId = unit.ACDId;
                HitpointsCurrent = unit.HitpointsCurrent;
            }
            public int UnitId { get; set; }
            public float HitpointsCurrent { get; set; }



        }
    }
}
