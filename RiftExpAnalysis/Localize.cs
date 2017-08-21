using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trinity.Framework;

namespace TinyZ.RiftExpAnalysis
{
    /// <summary>
    /// 本地化
    /// </summary>
    public static class Localize
    {

        //  const
        public const string PluginAuthor = "TinyZ.";
        public const string PluginDesc = "密境经验统计插件.";
        public const string PluginName = "密境经验统计插件.";
        public static readonly Version PluginVersison = new Version(1,0,0);
        public const string LogInitialized = "[经验统计插件]初始化完成.";

        //  language
        public static string TabHeader = "Rift Exp Analysis Plugin";
        public static string GreaterRift = "Greater Rift";
        public static string NephalemRift = "Nephalem Rift";
        public static string Avg = "Avg";
        public static string Max = "Max";
        public static string Min = "Min";
        public static string CostTime = "Cost Time";
        public static string Exp = "Exp";
        public static string CreateGame = "Create Game";
        public static string Death = "Death";
        public static string Count = "Count";
        public static string LevelUp = "Level Up";
        public static string RunningTime = "Running Time";
        public static string TotalExp = "Total Exp";
        public static string EveryHourExp = "Every Hour Exp";

        /// <summary>
        /// 初始化语言文件
        /// </summary>
        public static void Initialize()
        {
            var document = XDocument.Load("Lang_Zh-CN.xml");
            foreach (var xElement in document.Root.Elements())
            {
                try
                {
                    if (xElement.HasAttributes && xElement.Attribute("id") != null)
                    {
                        var property = xElement.Attribute("id").Value;
                        var fieldInfo = typeof(Localize).GetField(property, BindingFlags.Static);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(typeof(Localize), xElement.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.Logger.Warn("Unknown field. Error:", e);  
                }
            }
        }
    }

    public class Language
    {
        public string DefaultRunTime = "Run times: 0:0:0";
        public string DefaultTotalEXP = "Total EXP: 0";
        public string DefaultEXPPerHour = "EXP per hour: 0";
        public string DefaultDeath = "Death: 0";
        public string DefaultLevelUp = "Level Up: 0";
        public string DefaultCreateGames = "Create Games: 0";
        public string DefaultLeaveGames = "Leave Games: 0";
        public string DefaultRiftCount = "Rift Count: 0 Greater: 0 Nephalem: 0";
        public string DefaultBestGreaterTime = "Best Greater Time: 0:0:0";
        public string DefaultWorseGreaterTime = "Worse Greater Time: 0:0:0";
        public string DefaultBestGreaterEXP = "Best Greater EXP: 0";
        public string DefaultWorseGreaterEXP = "Worse Greater EXP: 0";
        public string DefaultBestNephalemTime = "Best Nephalem Time: 0:0:0";
        public string DefaultWorseNephalemTime = "Worse Nephalem Time: 0:0:0";
        public string DefaultBestNephalemEXP = "Best Nephalem EXP: 0";
        public string DefaultWorseNephalemEXP = "Worse Nephalem EXP: 0";

        public string TabHeader = "EXPCount";

        public string RunTime = "Run Time: ";
        public string TotalEXP = "Total Exp: ";
        public string EXPPerHour = "Exp Per Hour: ";
        public string Death = "Death: ";
        public string LevelUp = "Level Up: ";
        public string CreateGames = "Create Games: ";
        public string LeaveGames = "Leave Games: ";
        public string RiftCount = "Rift Count: ";
        public string Greater = " Greater: ";
        public string Nephalem = " Nephalem: ";
        public string BestGreaterTime = "Best Greater Time: ";
        public string WorseGreaterTime = "Worse Greater Time: ";
        public string BestGreaterEXP = "Best Greater EXP: ";
        public string WorseGreaterEXP = "Worse Greater EXP: ";
        public string BestNephalemTime = "Best Nephalem Time: ";
        public string WorseNephalemTime = "Worse Nephalem Time: ";
        public string BestNephalemEXP = "Best Nephalem EXP: ";
        public string WorseNephalemEXP = "Worse Nephalem EXP: ";

        public string PluginName = "经验统计插件1.1.0";
        public string PluginAuthor = "TinyZ, KillSB";
        public string PluginDescription = "经验统计插件";

        
    }

    public class CN : Language
    {
        public CN()
        {
            DefaultRunTime = "运行时间: 0:0:0";
            DefaultTotalEXP = "总共获得经验: 0";
            DefaultEXPPerHour = "每小时经验: 0";
            DefaultDeath = "死亡次数: 0";
            DefaultLevelUp = "总计升级次数: 0";
            DefaultCreateGames = "创建房间次数: 0";
            DefaultLeaveGames = "离开房间次数: 0";
            DefaultRiftCount = "秘境次数: 0  大秘境: 0  小秘境: 0";
            DefaultBestGreaterTime = "单次大秘境最快时间: 0:0:0";
            DefaultWorseGreaterTime = "单次大秘境最慢时间: 0:0:0";
            DefaultBestGreaterEXP = "单次大秘境最多经验: 0";
            DefaultWorseGreaterEXP = "单次大秘境最少经验: 0";
            DefaultBestNephalemTime = "单次小秘境最快时间: 0:0:0";
            DefaultWorseNephalemTime = "单次小秘境最慢时间: 0:0:0";
            DefaultBestNephalemEXP = "单次小秘境最多经验: 0";
            DefaultWorseNephalemEXP = "单次小秘境最少经验: 0";

            TabHeader = "经验统计";

            RunTime = "运行时间: ";
            TotalEXP = "总共获得经验: ";
            EXPPerHour = "每小时经验: ";
            Death = "死亡次数: ";
            LevelUp = "总计升级次数: ";
            CreateGames = "创建房间次数: ";
            LeaveGames = "离开房间次数: ";
            RiftCount = "秘境次数: ";
            Greater = " 大秘境: ";
            Nephalem = " 小秘境: ";
            BestGreaterTime = "单次大秘境最快时间: ";
            WorseGreaterTime = "单次大秘境最慢时间: ";
            BestGreaterEXP = "单次大秘境最多经验: ";
            WorseGreaterEXP = "单次大秘境最少经验: ";
            BestNephalemTime = "单次小秘境最快时间: ";
            WorseNephalemTime = "单次小秘境最慢时间: ";
            BestNephalemEXP = "单次小秘境最多经验: ";
            WorseNephalemEXP = "单次小秘境最少经验: ";

            LogSystemLoaded = "【暗黑插件爱好者协会QQ群319496921】插件加载成功";
        }
    }
}
