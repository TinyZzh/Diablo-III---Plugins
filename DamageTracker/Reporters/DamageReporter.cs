using System.Linq;
using StatsTracker.Trackers;
using StatsTracker.UI;

namespace StatsTracker.Reporters
{
    internal static class DamageReporter
    {
        internal static void UpdateStats()
        {
            Report();
            ReportLastXSeconds(10);
            TabUi.SetMaxDps(DpsTracker.GetMaxDps());
        }

        internal static void ReportCurrentDps()
        {
            var currentDps = CurrentDpsTracker.GetCurrentDps();
            TabUi.SetCurrentDps(currentDps);

        }

        internal static void Report()
        {
            var report = DpsTracker.GetReport();
            float totalDamage;
            float averageDps;
            if (report.Count == 0)
            {
                totalDamage = 0;
                averageDps = 0;
            }
            else
            {
                totalDamage = report.Sum();
                averageDps = report.Average();
            }
            TabUi.SetTotalDamage(totalDamage);
            TabUi.SetAverageDps(averageDps);
            //Logger.Info(string.Format("Total Damage Done: {0:N0}  Average Dps: {1:N0}", totalDamage, averageDps));
        }

        internal static void ReportLastXSeconds(long seconds)
        {
            var report = DpsTracker.GetLastSecondsReport(seconds);
            float totalDamage;
            float averageDps;
            if (report.Count == 0)
            {
                totalDamage = 0;
                averageDps = 0;
            }
            else
            {
                totalDamage = report.Sum();
                averageDps = report.Average();
            }
            TabUi.SetTotalDamageLast10(totalDamage);
            TabUi.SetAverageDpsLast10(averageDps);
            //Logger.Info(string.Format("Last {2} seconds -  Damage Done: {0:N0}  Average Dps: {1:N0}", totalDamage, averageDps, seconds));
        }

    }
}
