using System;
using System.Diagnostics;
using System.Windows;
using StatsTracker.Reporters;
using StatsTracker.Trackers;
using StatsTracker.UI;
using StatsTracker.Util;
using Zeta.Bot;
using Zeta.Common.Plugins;
using Zeta.Game;


namespace StatsTracker
{
    /// <summary>
    /// 
    /// 基于0.2.50615版本的伤害统计插件修改维护，以支持高版本的DB. 版本好重新命名
    /// 
    /// <example>
    /// 
    /// 
    /// logs: 2017-08-14   version: 1.0.0
    /// 1. 修复IsLoadingWorld和IsPlayingCutscene引用错误
    /// 2. UnitTracker中使用的ACDguId修改为ACDId
    /// 3. 增加本地语言支持. Local
    /// </example>
    /// </summary>
    public class DamageTracker : IPlugin
    {

        public string Name { get; private set; }
        public Version Version { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public Window DisplayWindow { get { return null; } }


        internal static Version PluginVersion = new Version(1, 0, 0);
        internal static bool IsBotStarted = false;

        private long _lastUpdate;

        public void OnPulse()
        {
            if (!IsBotStarted)
            {
                return;
            }

            if (IsGameOrBotPaused)
            {
                return;
            }

            if (ZetaDia.Me == null || ZetaDia.Me.CommonData == null || !ZetaDia.Me.IsValid || !ZetaDia.Me.CommonData.IsValid)
                return;

            if (!ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld || ZetaDia.Globals.IsPlayingCutscene || ZetaDia.IsInTown)
                return;

            UnitTracker.TrackUnits();
            var currentSecond = TimeProvider.CurrentSecond;
            if (_lastUpdate != currentSecond)
            {
                DamageReporter.UpdateStats();
                _lastUpdate = currentSecond;
            }
            DamageReporter.ReportCurrentDps();

        }

        public void OnInitialize()
        {
            Logger.Info(string.Format("({0}) initialized.", Version));
            BotMain.OnStart += OnBotStart;
            BotMain.OnStop += OnBotStop;

        }

        public void OnShutdown()
        {

        }

        public void OnEnabled()
        {
            TabUi.InstallTab();
            Logger.Info(string.Format("({0}) enabled.", Version));
        }

        public void OnDisabled()
        {
            TabUi.RemoveTab();
            Logger.Info((string.Format("({0}) disabled.", Version)));
        }

        internal static void OnBotStart(IBot bot)
        {
            IsBotStarted = true;
        }

        internal static void OnBotStop(IBot bot)
        {
            IsBotStarted = false;
        }


        private DamageTracker()
        {
            Name = Local.LocalPluginName;
            Version = PluginVersion;
            Author = "TinyZ, TarasBulba";
            Description = Local.LocalPluginDescription;
        }

        public bool Equals(IPlugin other)
        {
            return false;
        }

        internal static bool IsGameOrBotPaused
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BotMain.StatusText))
                {
                    return BotMain.StatusText.IndexOf("paused", StringComparison.InvariantCultureIgnoreCase) > 0;
                }
                return true;
            }
        }


    }

}
