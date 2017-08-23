using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Zeta.Game.Internals;

namespace TinyZ.RiftExpAnalysis
{
    public class TabUI
    {
        private static TabItem _tabItem;


        private static readonly Dictionary<string, TextBlock> _labels = new Dictionary<string, TextBlock>();


        internal static void InitTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    var mainWindow = Application.Current.MainWindow;

                    //  左面板
                    var leftPanel = new StackPanel
                    {
                        Background = Brushes.DimGray,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 176,
                        Margin = new Thickness(2, 2, 0, 2)
                    };
                    var left = new[]
                    {
                        Localize.RunningTime,
                        Localize.TotalExp,
                        Localize.EveryHourExp,
                        //  
                        Localize.GreaterRift + Localize.Death,
                        Localize.GreaterRift + Localize.Count,
                        Localize.GreaterRift + Localize.CostTime,
                        Localize.GreaterRift + Localize.Exp,
                    };
                    foreach (var t in left)
                    {
                        var textBlock = CreateLabel(t, HorizontalAlignment.Stretch);
                        _labels.Add(t, textBlock);
                        leftPanel.Children.Add(textBlock);
                    }

                    //  右面板
                    var rightPanel = new StackPanel
                    {
                        Background = Brushes.DimGray,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 176,
                        Margin = new Thickness(2, 2, 0, 2)
                    };
                    var right = new[]
                    {
                        Localize.Death,
                        Localize.LevelUp,
                        Localize.CreateGame,
                        //  
                        Localize.NephalemRift + Localize.Death,
                        Localize.NephalemRift + Localize.Count,
                        Localize.NephalemRift + Localize.CostTime,
                        Localize.NephalemRift + Localize.Exp
                    };
                    foreach (var t in right)
                    {
                        var textBlock = CreateLabel(t, HorizontalAlignment.Stretch);
                        _labels.Add(t, textBlock);
                        rightPanel.Children.Add(textBlock);
                    }

                    //  
                    var uniformGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                        MinWidth = 500,
                        Columns = 2
                    };

                    uniformGrid.Children.Add(leftPanel);
                    uniformGrid.Children.Add(rightPanel);

                    _tabItem = new TabItem
                    {
                        Header = Localize.TabHeader,
                        Content = uniformGrid
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;
                    tabs.Items.Add(_tabItem);
                });
        }


        private static TextBlock CreateLabel(string title, HorizontalAlignment haAlignment)
        {
            return new TextBlock
            {
                Text = title,
                Height = 18,
                //Padding = new Thickness(0, 2, 0, 0),
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = haAlignment,
                TextAlignment = TextAlignment.Left,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
            };
        }

        internal static void DestroyUi()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                if (tabs == null)
                    return;
                tabs.Items.Remove(_tabItem);
            });
        }

        /// <summary>
        /// 更新密境统计信息
        /// </summary>
        /// <param name="riftType">密境类型</param>
        internal static void UpdateRiftAnalysisInfo(RiftType riftType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var battleReports = RiftExpAnalysisPugin.GetRiftBattleReport(riftType);
                if (battleReports == null)
                    return;

                var rift = RiftType.Greater == riftType ? Localize.GreaterRift : Localize.NephalemRift;
                var fields = new[]
                {
                    Localize.Death, Localize.Count, Localize.CostTime, Localize.Exp
                };
                var detail = ":[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" + Localize.Min + ":{2}" +
                             Localize.Avg + ":{3}]";
                foreach (var field in fields)
                {
                    if (!_labels.ContainsKey(rift + field))
                    {
                        Logger.Error("字段不存在{0}", rift + field);
                        return;
                    }
                    if (field == Localize.Death)
                    {
                        _labels[rift + field].Text = string.Format(rift + field + detail,
                            RiftExpAnalysisPugin.GetTotalDeath(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMaxDeath(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMinDeath(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetAvgDeath(battleReports, RiftType.Greater)
                        );
                    }
                    else if (Localize.Count == field)
                    {
                        _labels[rift + field].Text = string.Format(rift + field + "{0}", battleReports.ToArray().Length);
                    }
                    else if (Localize.CostTime == field)
                    {
                        _labels[rift + field].Text = string.Format(rift + field + detail,
                            RiftExpAnalysisPugin.GetTotalCostTime(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMaxCostTime(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMinCostTime(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetAvgCostTime(battleReports, RiftType.Greater)
                        );
                    }
                    else if (Localize.Exp == field)
                    {
                        _labels[rift + field].Text = string.Format(rift + field + detail,
                            RiftExpAnalysisPugin.GetTotalExp(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMaxExp(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetMinExp(battleReports, RiftType.Greater),
                            RiftExpAnalysisPugin.GetAvgExp(battleReports, RiftType.Greater)
                        );
                    }
                }
            });
        }


        internal static void UpdateUi()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var worlds = new[]
                {
                    Localize.RunningTime,
                    Localize.TotalExp,
                    Localize.EveryHourExp,
                    Localize.Death,
                    Localize.LevelUp,
                    Localize.CreateGame,
                    //  
                    Localize.GreaterRift + Localize.Death,
                    Localize.GreaterRift + Localize.Count + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    Localize.GreaterRift + Localize.CostTime + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    Localize.GreaterRift + Localize.Exp + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    //
                    Localize.NephalemRift + Localize.Death + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    Localize.NephalemRift + Localize.Count + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    Localize.NephalemRift + Localize.CostTime + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]",
                    Localize.NephalemRift + Localize.Exp + "[" + Localize.Total + ":{0}" + Localize.Max + ":{1}" +
                    Localize.Min + ":{2}" + Localize.Avg + ":{3}]"
                };

                var ws = new Dictionary<string, string>();
                

                //
                //
                //
                //                _allTextBlocks
                //
                //
                //
                //
                //
                //
                //                TimeSpan taken = DateTime.Now - RiftExpAnalysisPugin.BootstarpDateTime;
                //                Texts[0].Text = Localize.RunningTime + " " + (taken.Hours + (taken.Days * 24)) +":" +taken.Minutes+ ":"+taken.Seconds;
                //                Texts[1].Text = Localize.TotalExp + RiftExpAnalysisPugin.GetSimpleEXP(RiftExpAnalysisPugin.TotalEXP);
                //                Texts[2].Text = Localize.EveryHourExp + RiftExpAnalysisPugin.GetSimpleEXP(GetEXPPerHour(taken, RiftExpAnalysisPugin.TotalEXP));
                //                Texts[3].Text = Localize.Death + RiftExpAnalysisPugin.DBDeath;
                //                Texts[4].Text = Localize.LevelUp + (RiftExpAnalysisPugin.StartLevel == 0 ? 0 : (ZetaDia.Me.ParagonLevel - RiftExpAnalysisPugin.StartLevel));
                //                Texts[5].Text = Localize.CreateGame + RiftExpAnalysisPugin.Creates;
                //                Texts[6].Text = Localize.LeaveGames + RiftExpAnalysisPugin.Leaves;
                //                Texts[7].Text = Localize.RiftCount + (RiftExpAnalysisPugin.GreaterRiftCount + RiftExpAnalysisPugin.NephalemRiftCount) + Localize.GetInstance().Greater + RiftExpAnalysisPugin.GreaterRiftCount + Localize.GetInstance().Nephalem + RiftExpAnalysisPugin.NephalemRiftCount;
                //
                //                IEnumerable<RiftExpAnalysisPugin.RiftEntry> results = RiftExpAnalysisPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Greater);
                //                if (results.FirstOrDefault() != null)
                //                {
                //                    RiftExpAnalysisPugin.RiftEntry temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetTakenTime() < temp.GetTakenTime())
                //                            temp = entry;
                //                    }
                //                    Texts[8].Text = Localize.GetInstance().BestGreaterTime + temp.GetTakenTimeSimple();
                //
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetTakenTime() > temp.GetTakenTime())
                //                            temp = entry;
                //                    }
                //                    Texts[9].Text = Localize.GetInstance().WorseGreaterTime + temp.GetTakenTimeSimple();
                //
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetExp() > temp.GetExp())
                //                            temp = entry;
                //                    }
                //                    Texts[10].Text = Localize.GetInstance().BestGreaterEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                //                    
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetExp() < temp.GetExp())
                //                            temp = entry;
                //                    }
                //                    Texts[11].Text = Localize.GetInstance().WorseGreaterEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                //                }
                //                else
                //                {
                //                    Texts[8].Text = Localize.GetInstance().DefaultBestGreaterTime;
                //                    Texts[9].Text = Localize.GetInstance().DefaultWorseGreaterTime;
                //                    Texts[10].Text = Localize.GetInstance().DefaultBestGreaterEXP;
                //                    Texts[11].Text = Localize.GetInstance().DefaultWorseGreaterEXP;
                //                }
                //
                //                results = RiftExpAnalysisPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Nephalem);
                //                if (results.FirstOrDefault() != null)
                //                {
                //                    RiftExpAnalysisPugin.RiftEntry temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetTakenTime() < temp.GetTakenTime())
                //                            temp = entry;
                //                    }
                //                    Texts[12].Text = Localize.GetInstance().BestNephalemTime + temp.GetTakenTimeSimple();
                //
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetTakenTime() > temp.GetTakenTime())
                //                            temp = entry;
                //                    }
                //                    Texts[13].Text = Localize.GetInstance().WorseNephalemTime + temp.GetTakenTimeSimple();
                //
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetExp() > temp.GetExp())
                //                            temp = entry;
                //                    }
                //                    Texts[14].Text = Localize.GetInstance().BestNephalemEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                //                    
                //                    temp = results.FirstOrDefault();
                //                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                //                    {
                //                        if (entry.GetExp() < temp.GetExp())
                //                            temp = entry;
                //                    }
                //                    Texts[15].Text = Localize.GetInstance().WorseNephalemEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                //                }
                //                else
                //                {
                //                    Texts[12].Text = Localize.GetInstance().DefaultBestNephalemTime;
                //                    Texts[13].Text = Localize.GetInstance().DefaultWorseNephalemTime;
                //                    Texts[14].Text = Localize.GetInstance().DefaultBestNephalemEXP;
                //                    Texts[15].Text = Localize.GetInstance().DefaultWorseNephalemEXP;
                //                }
            });
        }

        internal static long GetEXPPerHour(TimeSpan taken, long totalEXP)
        {
            var seconds = taken.TotalSeconds;
            var perEXP = totalEXP / seconds;
            return (long) (perEXP * 3600);
        }
    }
}