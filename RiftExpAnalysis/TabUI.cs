using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Zeta.Game;

namespace TinyZ.RiftExpAnalysis
{
    public class TabUI
    {
        static TextBlock[] Texts;
        private static TabItem _tabItem;


        private static ConcurrentDictionary<string, TextBlock> _allTextBlocks = new ConcurrentDictionary<string, TextBlock>();

        internal static void InitTab()
        {


            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Texts = new TextBlock[16];


                    var mainWindow = Application.Current.MainWindow;
                    //  
                    Dictionary<string, TextBlock> textBlocks = new Dictionary<string, TextBlock>();

                    //  左面板
                    var leftPanel = new StackPanel { Background = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 176, Margin = new Thickness(2, 2, 0, 2) };
                    var worlds = new[]
                    {
                        Localize .RunningTime,
                        Localize .TotalExp,
                        Localize .EveryHourExp,
                        Localize .Death,
                        Localize .LevelUp,
                        Localize .CreateGame,
                        Localize .GreaterRift + Localize.Count,
                        Localize .GreaterRift + Localize.CostTime,
                        Localize .GreaterRift + Localize.Exp,
                        Localize .NephalemRift + Localize.Count,
                        Localize .NephalemRift + Localize.CostTime,
                        Localize .NephalemRift + Localize.Exp,
                    };
                    for (var i = 0; i < worlds.Length; i++)
                    {
                        var textBlock = CreateLabel(worlds[i], HorizontalAlignment.Stretch);
                        _allTextBlocks[worlds[i]] = textBlock;
                        leftPanel.Children.Add(textBlock);
                    }
                    //  右面板


                    Texts[0] = CreateLabel(Localize .RunningTime, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[0]);

                    Texts[1] = CreateLabel(Localize .TotalExp, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[1]);

                    Texts[2] = CreateLabel(Localize .EveryHourExp, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[2]);

                    Texts[3] = CreateLabel(Localize .Death, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[3]);

                    Texts[4] = CreateLabel(Localize .DefaultLevelUp, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[4]);

                    Texts[5] = CreateLabel(Localize .DefaultCreateGames, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[5]);

                    Texts[6] = CreateLabel(Localize .DefaultLeaveGames, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[6]);

                    Texts[7] = CreateLabel(Localize .DefaultRiftCount, HorizontalAlignment.Stretch);
                    leftPanel.Children.Add(Texts[7]);

                    var RightPanel = new StackPanel { Background = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 176, Margin = new Thickness(2, 2, 0, 2) };

                    Texts[8] = CreateLabel(Localize .DefaultBestGreaterTime, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[8]);

                    Texts[9] = CreateLabel(Localize .DefaultWorseGreaterTime, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[9]);

                    Texts[10] = CreateLabel(Localize .DefaultBestGreaterEXP, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[10]);
                    
                    Texts[11] = CreateLabel(Localize .DefaultWorseGreaterEXP, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[11]);

                    Texts[12] = CreateLabel(Localize .DefaultBestNephalemTime, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[12]);

                    Texts[13] = CreateLabel(Localize .DefaultWorseNephalemTime, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[13]);

                    Texts[14] = CreateLabel(Localize .DefaultBestNephalemEXP, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[14]);

                    Texts[15] = CreateLabel(Localize .DefaultWorseNephalemEXP, HorizontalAlignment.Stretch);
                    RightPanel.Children.Add(Texts[15]);


                    var uniformGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                        MinWidth = 500,
                        Columns = 2
                    };

                    uniformGrid.Children.Add(leftPanel);
                    uniformGrid.Children.Add(RightPanel);

                    _tabItem = new TabItem
                    {
                        Header = Localize .TabHeader,
                        Content = uniformGrid,
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                    {
                        return;
                    }
                    
                    tabs.Items.Add(_tabItem);
                });
        }



        static TextBlock CreateLabel(string title, HorizontalAlignment haAlignment)
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

        internal static void destroyUI()
        {
            Application.Current.Dispatcher.Invoke(new System.Action(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                if (tabs == null)
                    return;
                tabs.Items.Remove(_tabItem);
            }));
        }

        internal static void updateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TimeSpan taken = DateTime.Now - RiftExpAnalysisPugin.StartTime;
                Texts[0].Text = Localize.RunningTime + " " + (taken.Hours + (taken.Days * 24)) +":" +taken.Minutes+ ":"+taken.Seconds;
                Texts[1].Text = Localize.TotalExp + RiftExpAnalysisPugin.GetSimpleEXP(RiftExpAnalysisPugin.TotalEXP);
                Texts[2].Text = Localize.EveryHourExp + RiftExpAnalysisPugin.GetSimpleEXP(GetEXPPerHour(taken, RiftExpAnalysisPugin.TotalEXP));
                Texts[3].Text = Localize.Death + RiftExpAnalysisPugin.DBDeath;
                Texts[4].Text = Localize.LevelUp + (RiftExpAnalysisPugin.StartLevel == 0 ? 0 : (ZetaDia.Me.ParagonLevel - RiftExpAnalysisPugin.StartLevel));
                Texts[5].Text = Localize.CreateGame + RiftExpAnalysisPugin.Creates;
                Texts[6].Text = Localize.LeaveGames + RiftExpAnalysisPugin.Leaves;
                Texts[7].Text = Localize.RiftCount + (RiftExpAnalysisPugin.GreaterRiftCount + RiftExpAnalysisPugin.NephalemRiftCount) + Localize.GetInstance().Greater + RiftExpAnalysisPugin.GreaterRiftCount + Localize.GetInstance().Nephalem + RiftExpAnalysisPugin.NephalemRiftCount;

                IEnumerable<RiftExpAnalysisPugin.RiftEntry> results = RiftExpAnalysisPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Greater);
                if (results.FirstOrDefault() != null)
                {
                    RiftExpAnalysisPugin.RiftEntry temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() < temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[8].Text = Localize.GetInstance().BestGreaterTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() > temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[9].Text = Localize.GetInstance().WorseGreaterTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetExp() > temp.GetExp())
                            temp = entry;
                    }
                    Texts[10].Text = Localize.GetInstance().BestGreaterEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                    
                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetExp() < temp.GetExp())
                            temp = entry;
                    }
                    Texts[11].Text = Localize.GetInstance().WorseGreaterEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                }
                else
                {
                    Texts[8].Text = Localize.GetInstance().DefaultBestGreaterTime;
                    Texts[9].Text = Localize.GetInstance().DefaultWorseGreaterTime;
                    Texts[10].Text = Localize.GetInstance().DefaultBestGreaterEXP;
                    Texts[11].Text = Localize.GetInstance().DefaultWorseGreaterEXP;
                }

                results = RiftExpAnalysisPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Nephalem);
                if (results.FirstOrDefault() != null)
                {
                    RiftExpAnalysisPugin.RiftEntry temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() < temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[12].Text = Localize.GetInstance().BestNephalemTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() > temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[13].Text = Localize.GetInstance().WorseNephalemTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetExp() > temp.GetExp())
                            temp = entry;
                    }
                    Texts[14].Text = Localize.GetInstance().BestNephalemEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                    
                    temp = results.FirstOrDefault();
                    foreach (RiftExpAnalysisPugin.RiftEntry entry in results)
                    {
                        if (entry.GetExp() < temp.GetExp())
                            temp = entry;
                    }
                    Texts[15].Text = Localize.GetInstance().WorseNephalemEXP + RiftExpAnalysisPugin.GetSimpleEXP(temp.GetExp());
                }
                else
                {
                    Texts[12].Text = Localize.GetInstance().DefaultBestNephalemTime;
                    Texts[13].Text = Localize.GetInstance().DefaultWorseNephalemTime;
                    Texts[14].Text = Localize.GetInstance().DefaultBestNephalemEXP;
                    Texts[15].Text = Localize.GetInstance().DefaultWorseNephalemEXP;
                }
            });
        }

        internal static long GetEXPPerHour(TimeSpan taken, long totalEXP)
        {
            double seconds = taken.TotalSeconds;
            double perEXP = totalEXP / seconds;
            return (long)(perEXP * 3600);
        }
    }
}
