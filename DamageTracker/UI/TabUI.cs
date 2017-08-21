using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using StatsTracker.Reporters;
using StatsTracker.Trackers;
using StatsTracker.Util;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace StatsTracker.UI
{
    class TabUi
    {

        private static TabItem _tabItem;
        private static Label _lblTotalDamage;
        private static Label _lblAverageDps;
        private static Label _lblTotalDamage10;
        private static Label _lblAverageDps10;
        private static Label _lblCurrentDps;
        private static Label _lblMaxDps;

        internal static void InstallTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {

                    Window mainWindow = Application.Current.MainWindow;

                    var tabGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                        Height = 180

                    };
                    tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
                    tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    tabGrid.RowDefinitions.Add(new RowDefinition());

                    var groupBox = new GroupBox();
                    groupBox.Header = "Damage Stats";
                    var uniformGrid = new UniformGrid();
                    uniformGrid.Columns = 2;

                    uniformGrid.Children.Add(CreateLabel(Local.LocalTotalDamage));
                    _lblTotalDamage = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblTotalDamage);

                    uniformGrid.Children.Add(CreateLabel(Local.LocalAverageDps));
                    _lblAverageDps = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblAverageDps);

                    uniformGrid.Children.Add(CreateLabel(Local.LocalMaxDps));
                    _lblMaxDps = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblMaxDps);
                    
                    uniformGrid.Children.Add(CreateLabel(Local.LocalTotalDamage10Sec));
                    _lblTotalDamage10 = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblTotalDamage10);

                    uniformGrid.Children.Add(CreateLabel(Local.LocalAverageDps10Sec));
                    _lblAverageDps10 = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblAverageDps10);

                    uniformGrid.Children.Add(CreateLabel(Local.LocalCurrentDps));
                    _lblCurrentDps = CreateLabel("0", HorizontalAlignment.Right);
                    uniformGrid.Children.Add(_lblCurrentDps);


                    uniformGrid.Children.Add(new TextBlock());

                    var resetDamageButton = CreateButton(Local.LocalResetDamageStats, ResetDamageStats);
                    resetDamageButton.HorizontalAlignment = HorizontalAlignment.Center;
                    uniformGrid.Children.Add(resetDamageButton);

                    groupBox.Content = uniformGrid;
                    Grid.SetColumn(groupBox, 0);

                    var groupBox2 = new GroupBox();
                    groupBox2.Header = "Other Stats (Coming Soon)";
                    groupBox2.Width=400;

                    Grid.SetColumn(groupBox2, 1);


                    tabGrid.Children.Add(groupBox);
                    tabGrid.Children.Add(groupBox2);


                    _tabItem = new TabItem
                    {
                        Header = Local.LocalPluginName,
                        ToolTip = Local.LocalPluginTabToolTip,
                        Content = tabGrid,
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;

                    tabs.Items.Add(_tabItem);

                }
            );
        }

        internal static void SetTotalDamage(float damage)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblTotalDamage.Content = Local.ShowDamage(damage);
                });
        }

        internal static void SetAverageDps(float dps)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblAverageDps.Content = Local.ShowDamage(dps);
                });
        }

        internal static void SetTotalDamageLast10(float damage)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblTotalDamage10.Content = Local.ShowDamage(damage);
                });
        }

        internal static void SetAverageDpsLast10(float dps)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblAverageDps10.Content = Local.ShowDamage(dps);
                });
        }

        internal static void SetCurrentDps(float dps)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblCurrentDps.Content = Local.ShowDamage(dps);
                });
        }
        internal static void SetMaxDps(float dps)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    _lblMaxDps.Content = Local.ShowDamage(dps);
                });
        }

        internal static void RemoveTab()
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Window mainWindow = Application.Current.MainWindow;
                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                        return;
                    tabs.Items.Remove(_tabItem);
                }
            );
        }

        private static Button CreateButton(string buttonText, RoutedEventHandler clickHandler)
        {
            var button = new Button
            {
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0),
                Content = buttonText
            };
            button.Click += clickHandler;
            return button;
        }

        private static Label CreateLabel(string labelText, HorizontalAlignment textAlignment = HorizontalAlignment.Left)
        {
            var label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2),
                Content = labelText,
                HorizontalContentAlignment = textAlignment,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            };
            return label;
        }

        private static TextBlock CreateTextBlock(string labelText, HorizontalAlignment textAlignment = HorizontalAlignment.Left)
        {
            var label = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2),
                Text = labelText,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
            };
            return label;
        }

        private static void ResetDamageStats(object sender, RoutedEventArgs e)
        {
            try
            {
                DpsTracker.Reset();
                CurrentDpsTracker.Reset();
                DamageReporter.Report();
                DamageReporter.ReportCurrentDps();
                DamageReporter.ReportLastXSeconds(10);
            }
            catch (Exception ex)
            {
                
                Logger.Error("Error reseting damage stats: " + ex);
            }            
        }


        /**************
         * 
         * WARNING
         * 
         * ALWAYS surround your RoutedEventHandlers in try/catch. Failure to do so will result in Demonbuddy CRASHING if an exception is thrown.
         * 
         * WARNING
         *  
         *************/



        //private static void ReloadItemRulesEventHandler(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (Trinity.StashRule == null)
        //            Trinity.StashRule = new ItemRules.Interpreter();

        //        if (Trinity.StashRule != null)
        //        {
        //            BotMain.PauseWhile(Trinity.StashRule.reloadFromUI);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Error Reloading Item Rules:" + ex);
        //    }
        //}


    }
}
