using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Trinity.Framework;
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

        public static long DBDeath;
        public static long LastEXP;
        public static long LastLevelEXP;
        public static long TotalEXP;
        public static DateTime StartTime;
        public static long LastLevel;
        public static long StartLevel;
        public static long Creates;
        public static long Leaves;

        public static RiftEntry CurrentRift;
        public static int GreaterRiftCount;
        public static int NephalemRiftCount;
        public static int RiftSequenceNumber;
        public static List<RiftEntry> RiftList = new List<RiftEntry>();

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
        ///     当前密境类型
        /// </summary>
        public static RiftType CurrentRiftType = RiftType.None;

        /// <summary>
        ///     当前密境战报
        /// </summary>
        public static volatile RiftBattleReport Current;

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

        public Version Version => new Version(1, 0, 4);

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
            Logger.System(Localize.LogInitialized);
        }

        /// <summary>
        ///     启用插件
        /// </summary>
        public void OnEnabled()
        {
            if (IsServiceRunning)
                return;
            GameEvents.OnGameJoined += OnGameJoined;
            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnPlayerDied;
            BotMain.OnStart += OnStart;
            BotMain.OnStop += OnStop;
            TabUI.InitTab();
            //
            IsServiceRunning = true;

            OnRiftStarted += RiftExpAnalysisPugin_OnRiftStarted;
            OnRiftCompleted += RiftExpAnalysisPugin_OnRiftCompleted;
            GameEvents.OnPlayerDied += GameEvents_OnPlayerDied;

            Logger.Info("[OnEnabled]");
        }
        
        /// <summary>
        ///     禁用插件
        /// </summary>
        public void OnDisabled()
        {
            if (!IsServiceRunning)
                return;
            GameEvents.OnGameJoined -= OnGameJoined;
            GameEvents.OnGameLeft -= OnGameLeft;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnLevelUp -= OnPlayerDied;
            BotMain.OnStart -= OnStart;
            BotMain.OnStop -= OnStop;
            TabUI.destroyUI();
            //  
            IsServiceRunning = false;

            OnRiftStarted -= RiftExpAnalysisPugin_OnRiftStarted;
            OnRiftCompleted -= RiftExpAnalysisPugin_OnRiftCompleted;

            Logger.Info("[OnDisabled]");
        }

        /// <summary>
        ///     心跳
        /// </summary>
        public void OnPulse()
        {
            AnalysisExpChange();
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
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public void OnShutdown()
        {
            IsServiceRunning = false;
            Logger.Info("[OnShutdown]");
        }

        #region 事件

        private void RiftExpAnalysisPugin_OnRiftStarted(object sender, EventArgs e)
        {
            Current = new RiftBattleReport
            {
                StartTime = DateTime.Now,
                RiftType = ZetaDia.Storage.CurrentRiftType
            };
            //
            LastParagonLevel = ZetaDia.Me.ParagonLevel;
            LastParagonExp = ZetaDia.Me.ParagonCurrentExperience;
            LastParagonNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;
        }

        private void RiftExpAnalysisPugin_OnRiftCompleted(object sender, EventArgs e)
        {
            var queue = BattleReports[Current.RiftType] ?? new ConcurrentQueue<RiftBattleReport>();
            queue.Enqueue(Current);
            Current = null;
        }

        private void GameEvents_OnPlayerDied(object sender, EventArgs e)
        {
            if (Current != null)
            {
                Current.TotalDeath++;
            }
        }

        #endregion

        /// <summary>
        ///     获取完成的战斗
        /// </summary>
        /// <param name="riftType">密境类型</param>
        /// <returns></returns>
        public static IEnumerable<RiftBattleReport> GetRiftBattleReport(RiftType riftType)
        {
            return BattleReports[riftType].Where(bp => bp.EndTime > bp.StartTime && bp.TotalExp > 0);
        }


        public void OnUpdateRiftBattleReport()
        {
            //  密境开始
            if (IsRiftStarted())
            {
                Current = new RiftBattleReport();
                CurrentRiftType = ZetaDia.Storage.CurrentRiftType;
            }

            //  密境结束
            if (Current != null && IsRiftCompleted())
            {
                var queue = BattleReports.ContainsKey(CurrentRiftType)
                    ? BattleReports[CurrentRiftType]
                    : new ConcurrentQueue<RiftBattleReport>();
                if (BattleReports.ContainsKey(CurrentRiftType))
                    BattleReports[CurrentRiftType] = queue;
                if (queue.Count > 100)
                {
                    RiftBattleReport rbp;
                    queue.TryDequeue(out rbp);
                    if (rbp != null)
                        Core.Logger.Warn("密境战报超过100条， 移除最老的数据. ");
                }
                //  保存战报
                queue.Enqueue(Current);
                Current = null;
            }
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
                {
                    Current.TotalExp += curExp - LastParagonExp;
                }
                else if (curLv > LastParagonLevel)  
                {
                    if (curLv - LastParagonLevel == 1) //  等级提升一级
                    {
                        Current.TotalExp += (LastParagonNextLevel - LastParagonExp) + curExp;
                        Current.LevelUp += 1;
                    }
                    else
                    {
                        //  TODO: 升级过多 - 只记录等级变更
                        Current.LevelUp += curLv - LastParagonLevel;
                        Core.Logger.Warn(string.Format(Localize.LevelUpWith, (curLv - LastParagonLevel)));
                    }
                }
                LastParagonExp = curExp;
                LastParagonLevel = curLv;
                LastParagonNextLevel = curExpNextLv;
                








                var paragonCurrentExperience = ZetaDia.Me.ParagonCurrentExperience;
                var paragonExperienceNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;
                long paragonLevel = ZetaDia.Me.ParagonLevel;
                if (paragonCurrentExperience == 0 || paragonExperienceNextLevel == 0 || paragonLevel == 0)
                    return;

                if (LastEXP == 0)
                {
                    DBDeath = 0;
                    StartTime = DateTime.Now;
                    LastEXP = paragonCurrentExperience;
                    LastLevelEXP = paragonExperienceNextLevel;
                    TotalEXP = 0;
                    StartLevel = paragonLevel;
                    Creates = 0;
                }

                var currentExp = paragonCurrentExperience;
                if (LastLevel != 0 && paragonLevel > LastLevel)
                    TotalEXP += LastLevelEXP + currentExp;

                if (LastEXP < currentExp)
                    TotalEXP += currentExp - LastEXP;

                LastEXP = currentExp;
                LastLevelEXP = paragonExperienceNextLevel;
                LastLevel = paragonLevel;
                UpdateRift();
                TabUI.updateUI();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        /// <summary>
        ///     更新密境
        /// </summary>
        public void UpdateRift()
        {
            if (ZetaDia.Storage.CurrentRiftType != RiftType.None)
                if (CurrentRift == null)
                {
                    if (IsRiftInvalid())
                        return;

                    CreateNewRift();
                }
                else
                {
                    // 如果秘境已经启动， 并且未完成
                    if (IsRiftUnderway() && IsNewRift())
                    {
                        UpdateRiftMissionExp();
                        CreateNewRift();
                    }

                    if (!IsNewRift() && IsRiftCompleted())
                    {
                        CurrentRift.EndTime = DateTime.Now;
                        CurrentRift.EndExp = TotalEXP;
                        CurrentRift.IsCompleted = true;
                        RiftSequenceNumber++;
                    }
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

        public void UpdateRiftMissionExp()
        {
            if (CurrentRift.EndExp != TotalEXP)
                CurrentRift.EndExp = TotalEXP;
        }

        public bool IsNewRift()
        {
            return CurrentRift.RiftSequenceNumber != RiftSequenceNumber;
        }


        public bool IsRiftUnderway()
        {
            return ZetaDia.Storage.RiftStarted && !ZetaDia.Storage.RiftCompleted;
        }

        public void CreateNewRift()
        {
            CurrentRift = new RiftEntry(RiftSequenceNumber);
            RiftList.Add(CurrentRift);
            RiftCountUp();
        }

        public void RiftCountUp()
        {
            if (CurrentRift.RiftType == RiftType.Greater)
                GreaterRiftCount++;

            if (CurrentRift.RiftType == RiftType.Nephalem)
                NephalemRiftCount++;
        }

        public void OnGameJoined(object sender, EventArgs eventArgs)
        {
            Creates++;
        }

        public void OnGameLeft(object sender, EventArgs eventArgs)
        {
            Leaves++;
        }

        public void OnPlayerDied(object sender, EventArgs eventArgs)
        {
            DBDeath++;
        }

        public void OnStart(IBot bot)
        {
            Logger.Warn("[OnStart]");
        }

        public void OnStop(IBot bot)
        {
            Logger.Warn("[OnStop]");
            DBDeath = 0;
            StartTime = DateTime.Now;
            LastEXP = 0;
            TotalEXP = 0;
            LastLevelEXP = 0;
            LastLevel = 0;
            StartLevel = 0;
            Creates = 0;
            Leaves = 0;
            CurrentRift = null;
            GreaterRiftCount = 0;
            NephalemRiftCount = 0;
            RiftSequenceNumber = 0;
            RiftList.Clear();
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
            ///     获得的总经验值.    每次升级需要累加经验值
            /// </summary>
            public long TotalExp;

            /// <summary>
            ///     玩家死亡次数
            /// </summary>
            public long TotalDeath;
        }


        public class RiftEntry
        {
            public long EndExp;
            public DateTime EndTime;
            public bool HasGuardianSpawned;
            public bool IsCompleted;
            public bool IsStarted;
            public int RiftLevel;
            public int RiftSequenceNumber;
            public RiftType RiftType;
            public long StartExp;
            public DateTime StartTime;

            public RiftEntry(int number)
            {
                RiftType = ZetaDia.Storage.CurrentRiftType;
                IsStarted = ZetaDia.Storage.RiftStarted;
                IsCompleted = ZetaDia.Storage.RiftCompleted;
                HasGuardianSpawned = ZetaDia.Storage.RiftGuardianSpawned;
                RiftLevel = ZetaDia.Storage.CurrentRiftLevel;
                StartTime = DateTime.Now;
                EndTime = DateTime.Now;
                StartExp = TotalEXP;
                RiftSequenceNumber = number;
            }

            public double GetTakenTime()
            {
                return (EndTime - StartTime).TotalSeconds;
            }

            public string GetTakenTimeSimple()
            {
                var taken = EndTime - StartTime;
                return taken.Hours + ":" + taken.Minutes + ":" + taken.Seconds;
            }

            public long GetExp()
            {
                return EndExp - StartExp;
            }

            public override string ToString()
            {
                return "[RiftType=" + RiftType + " IsStarted=" + Convert.ToSingle(IsStarted) + " IsCompleted=" +
                       Convert.ToString(IsCompleted) + " HasGuardianSpawned=" + Convert.ToString(HasGuardianSpawned) +
                       " RiftLevel=" + Convert.ToString(RiftLevel) +
                       " StartTime=" + StartTime.ToShortDateString() + " EndTime=" + EndTime.ToShortDateString() +
                       " StartEXP=" + Convert.ToString(StartExp) + " EndEXP=" + Convert.ToString(EndExp) + "]";
            }
        }

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

        public double GetMaxCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Max(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        public double GetMinCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Min(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        public double GetAvgCostTime(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Average(bp => (bp.EndTime - bp.StartTime).TotalMilliseconds);
        }

        #endregion

        #region 密境经验值

        public double GetMaxExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Max(bp => bp.TotalExp);
        }

        public double GetMinExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Min(bp => bp.TotalExp);
        }

        public double GetAvgExp(IEnumerable<RiftBattleReport> source, RiftType riftType)
        {
            return source.Average(bp => bp.TotalExp);
        }

        #endregion
    }
}