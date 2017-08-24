using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Zeta.Bot;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals;

namespace TinyZ.RiftExpAnalysis
{
    /// <summary>
    ///     密境经验统计插件 By TinyZ.
    /// </summary>
    public class RiftExpAnalysisPugin : IPlugin
    {
        /// <summary>
        ///     面板刷新时间间隔(单位:毫秒)
        /// </summary>
        private const int UpdateInterval = 1000;

        /// <summary>
        ///     当前巅峰等级
        /// </summary>
        public static int LastParagonLevel;

        /// <summary>
        ///     当前巅峰经验值
        /// </summary>
        public static long LastParagonExp;

        /// <summary>
        ///     巅峰等级下一级全部经验
        /// </summary>
        public static long LastParagonNextLevel;

        /// <summary>
        ///     当前密境战报
        /// </summary>
        public static volatile RiftBattleReport Current;

        /// <summary>
        /// 
        /// </summary>
        public static DateTime BootstarpDateTime;

        /// <summary>
        /// 启动时等级
        /// </summary>
        public static int BootstarpLevel;

        /// <summary>
        ///     密境战报
        /// </summary>
        public static ConcurrentDictionary<RiftType, ConcurrentQueue<RiftBattleReport>> BattleReports =
            new ConcurrentDictionary<RiftType, ConcurrentQueue<RiftBattleReport>>();

        /// <summary>
        ///     服务是否运行
        /// </summary>
        public static volatile bool IsServiceRunning;

        /// <summary>
        ///     最后一次刷新面板时间
        /// </summary>
        public static long LastRefreshTime;


        public string Author => Localize.PluginAuthor;

        public Version Version => Localize.PluginVersison;

        public string Name => Localize.PluginName;

        public string Description => Localize.PluginDesc;

        public Window DisplayWindow => null;

        public bool Equals(IPlugin other)
        {
            return other != null && Name.Equals(other.Name) && Author.Equals(other.Author) &&
                   Version.Equals(other.Version);
        }

        /// <summary>
        ///     DemonBuddy启动时初始化插件
        /// </summary>
        public void OnInitialize()
        {
            Localize.Initialize();
            BattleReports[RiftType.Greater] = new ConcurrentQueue<RiftBattleReport>();
            BattleReports[RiftType.Nephalem] = new ConcurrentQueue<RiftBattleReport>();
            Logger.System(Localize.LogInitialized);
        }

        /// <summary>
        ///     启用插件
        /// </summary>
        public void OnEnabled()
        {
            if (IsServiceRunning)
                return;
            Logger.Info("[OnEnabled] Start.");
            Localize.Initialize();
            IsServiceRunning = true;
            BootstarpDateTime = DateTime.Now;
            BootstarpLevel = ZetaDia.Me.ParagonLevel;

            OnRiftStarted += RiftExpAnalysisPugin_OnRiftStarted;
            OnRiftCompleted += RiftExpAnalysisPugin_OnRiftCompleted;
            GameEvents.OnPlayerDied += GameEvents_OnPlayerDied;
            //            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnPlayerDied;
//            BotMain.OnStart += OnStart;
//            BotMain.OnStop += OnStop;
            TabUI.InitTab();
            //
            Logger.Info("[OnEnabled] Completed.");
        }

        /// <summary>
        ///     禁用插件
        /// </summary>
        public void OnDisabled()
        {
            if (!IsServiceRunning)
                return;
            IsServiceRunning = false;

            OnRiftStarted -= RiftExpAnalysisPugin_OnRiftStarted;
            OnRiftCompleted -= RiftExpAnalysisPugin_OnRiftCompleted;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnLevelUp -= OnPlayerDied;
//            BotMain.OnStart -= OnStart;
//            BotMain.OnStop -= OnStop;
            TabUI.DestroyUi();
            //  
            Logger.Info("[OnDisabled]");
        }

        /// <summary>
        ///     心跳
        /// </summary>
        public void OnPulse()
        {
            //  密境 - 开启
            var isStart = ZetaDia.Storage.RiftStarted;
            if (isStart && !_riftStatus)
            {
                _riftStatus = isStart;
                if (OnRiftStarted != null)
                    OnRiftStarted(this, null);
            }
            //  密境 - 结束
            var isEnd = ZetaDia.Storage.RiftCompleted;
            if (isEnd && _riftStatus)
            {
                _riftStatus = isEnd;
                if (OnRiftCompleted != null)
                    OnRiftCompleted(this, null);
            }
            AnalysisExpChange();
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public void OnShutdown()
        {
            IsServiceRunning = false;
            Logger.Info("[OnShutdown]");
        }

        /// <summary>
        ///     获取完成的战斗
        /// </summary>
        /// <param name="riftType">密境类型</param>
        /// <returns></returns>
        public static IEnumerable<RiftBattleReport> GetRiftBattleReport(RiftType riftType)
        {
            if (!BattleReports.ContainsKey(riftType))
                return null;
            return BattleReports[riftType].Where(bp => bp.EndTime > bp.StartTime && bp.Exp > 0);
        }

        /// <summary>
        ///     是否密境已经开启
        /// </summary>
        /// <returns></returns>
        public static bool IsRiftStarted()
        {
            return ZetaDia.Storage.RiftStarted;
        }

        /// <summary>
        ///     是否密境已经完成
        /// </summary>
        /// <returns></returns>
        public bool IsRiftCompleted()
        {
            return ZetaDia.Storage.RiftStarted && ZetaDia.Storage.RiftCompleted;
        }

        /// <summary>
        ///     统计经验值变更信息
        /// </summary>
        private void AnalysisExpChange()
        {
            if (!IsServiceRunning)
                return;
            if (Current == null)
                return;
            var now = DateTime.Now.Ticks;
            var ts = new TimeSpan(Math.Max(0, now - LastRefreshTime));
            if (ts.TotalMilliseconds < UpdateInterval)
                return;
            LastRefreshTime = now;
            try
            {
                if (ZetaDia.Me == null || !ZetaDia.Me.IsValid || !ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld)
                    return;
                //  未满级
                var curExp = ZetaDia.Me.ParagonCurrentExperience;
                var curExpNextLv = ZetaDia.Me.ParagonExperienceNextLevel;
                var curLv = ZetaDia.Me.ParagonLevel;
                if (curExp <= 0 && curExpNextLv <= 0 && curLv <= 0)
                    return;
                //  等级未变更,经验值变更
                if (curLv == LastParagonLevel && curExp > LastParagonExp)
                    Current.Exp += curExp - LastParagonExp;
                else if (curLv > LastParagonLevel)
                    if (curLv - LastParagonLevel == 1) //  等级提升一级
                    {
                        Current.Exp += LastParagonNextLevel - LastParagonExp + curExp;
                        Current.LevelUp += 1;
                    }
                    else
                    {
                        //  TODO: 升级过多 - 只记录等级变更
                        Current.LevelUp += curLv - LastParagonLevel;
                        Logger.Warn(string.Format(Localize.LevelUpWith, curLv - LastParagonLevel));
                    }
                LastParagonExp = curExp;
                LastParagonLevel = curLv;
                LastParagonNextLevel = curExpNextLv;
                //  
                TabUI.UpdateBaseInfo();
                TabUI.UpdateRiftAnalysisInfo(RiftType.Greater);
                TabUI.UpdateRiftAnalysisInfo(RiftType.Nephalem);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public bool IsRiftInvalid()
        {
            if (ZetaDia.Storage.CurrentRiftType == RiftType.Nephalem ||
                ZetaDia.Storage.CurrentRiftType == RiftType.Greater)
            {
                if (ZetaDia.Storage.RiftStarted || ZetaDia.Storage.RiftCompleted)
                    return false;

                return true;
            }
            return false;
        }

        public bool IsRiftUnderway()
        {
            return ZetaDia.Storage.RiftStarted && !ZetaDia.Storage.RiftCompleted;
        }

        public void OnPlayerDied(object sender, EventArgs eventArgs)
        {
            if (Current != null)
            {
                Current.DeathCount += 1;
            }
        }

        public static string GetSimpleEXP(long exp)
        {
            if (CultureInfo.InstalledUICulture.Name.ToLower().StartsWith("zh"))
                return GetCNSimpleEXP(exp);

            return GetENSimpleEXP(exp);
        }

        public static string GetCNSimpleEXP(long exp)
        {
            var result = "" + exp % 10000;
            if (exp > 10000)
                result = exp % 100000000 / 10000 + "万" + result;
            if (exp > 100000000)
                result = exp / 100000000 + "亿" + result;

            return result;
        }

        public static string GetENSimpleEXP(long exp)
        {
            return exp.ToString("N0");
        }

        /// <summary>
        ///     密境战斗日志
        /// </summary>
        public class RiftBattleReport
        {
            /// <summary>
            ///     密境结束时间
            /// </summary>
            public DateTime EndTime;

            /// <summary>
            ///     密境中升级次数
            /// </summary>
            public int LevelUp;

            /// <summary>
            ///     密境类型
            /// </summary>
            public RiftType RiftType;

            /// <summary>
            ///     密境开启时间
            /// </summary>
            public DateTime StartTime;

            /// <summary>
            ///     玩家死亡次数
            /// </summary>
            public int DeathCount;

            /// <summary>
            ///     获得的总经验值.    每次升级需要累加经验值
            /// </summary>
            public long Exp;
        }

        #region 事件

        private void RiftExpAnalysisPugin_OnRiftStarted(object sender, EventArgs e)
        {
            Current = new RiftBattleReport
            {
                StartTime = DateTime.Now,
                RiftType = ZetaDia.Storage.CurrentRiftType
            };
            var queue = BattleReports[Current.RiftType] ?? new ConcurrentQueue<RiftBattleReport>();
            queue.Enqueue(Current);
            //
            LastParagonLevel = ZetaDia.Me.ParagonLevel;
            LastParagonExp = ZetaDia.Me.ParagonCurrentExperience;
            LastParagonNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;
        }

        private void RiftExpAnalysisPugin_OnRiftCompleted(object sender, EventArgs e)
        {
            Current = null;
        }

        private void GameEvents_OnPlayerDied(object sender, EventArgs e)
        {
            if (Current != null)
                Current.DeathCount++;
        }

        #endregion

        #region 事件

        private static bool _riftStatus;

        /// <summary>
        ///     密境开启事件
        /// </summary>
        public static event EventHandler<EventArgs> OnRiftStarted;

        /// <summary>
        ///     密境结束事件
        /// </summary>
        public static event EventHandler<EventArgs> OnRiftCompleted;

        #endregion

        #region 密境消耗时间

        public static double GetTotalCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Sum(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        public static double GetMaxCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Max(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        public static double GetMinCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Min(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        public static double GetAvgCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Average(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        #endregion

        #region 密境经验值

        public static double GetTotalExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Sum(bp => bp.Exp);
        }

        public static double GetMaxExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Max(bp => bp.Exp);
        }

        public static double GetMinExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Min(bp => bp.Exp);
        }

        public static double GetAvgExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Average(bp => bp.Exp);
        }

        #endregion

        #region 密境死亡次数

        /// <summary>
        ///     总死亡次数
        /// </summary>
        public static double GetTotalDeath(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Sum(bp => bp.DeathCount);
        }

        /// <summary>
        ///     最多死亡次数
        /// </summary>
        public static double GetMaxDeath(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Max(bp => bp.DeathCount);
        }

        /// <summary>
        ///     最少死亡次数
        /// </summary>
        public static double GetMinDeath(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Min(bp => bp.DeathCount);
        }

        /// <summary>
        ///     平均死亡次数
        /// </summary>
        public static double GetAvgDeath(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Average(bp => bp.DeathCount);
        }

        #endregion
    }
}