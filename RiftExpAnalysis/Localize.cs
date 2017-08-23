using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace TinyZ.RiftExpAnalysis
{
    /// <summary>
    ///     本地化
    /// </summary>
    public class Localize
    {
        /// <summary>
        ///     初始化语言文件
        /// </summary>
        public static void Initialize()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var pluginPath = Path.Combine(assemblyPath, "Plugins");
            var file = GetFile(pluginPath, "Lang_Zh-CN.xml");
            Logger.Warn(pluginPath);
            Logger.Warn(file.ToString());
            if (!File.Exists(file))
                return;
            var document = XDocument.Load(file);
            Logger.Warn(document.Root.Value);
            foreach (var xElement in document.Root.Elements())
            {
                try
                {
                    Logger.Debug("HasAttributes : ", xElement.HasAttributes);
                    Logger.Debug("Id : ", xElement.Attribute("id"));
                    if (xElement.HasAttributes && xElement.Attribute("id") != null)
                    {
                        var property = xElement.Attribute("id").Value;
                        var fieldInfo = typeof(Localize).GetField(property, BindingFlags.Static);
                        Logger.Debug("fieldInfo", fieldInfo);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(typeof(Localize), xElement.Value);
                            Logger.Debug("fieldInfo", fieldInfo.GetValue(typeof(Localize)));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn($"Unknown field. Error:{e.Message}");
                }
            }
            Logger.Info("Init Rift Exp Analysis Plugin 's Localize File Completed.");
        }


        #region Const Property

        //  const
        public static string PluginAuthor { get; } = "TinyZ.";

        public static string PluginDesc { get; } = " By TinyZ. 不定期维护. QQ:";
        public static string PluginName { get; } = "密境经验统计插件.";
        public static Version PluginVersison { get; } = new Version(1, 0, 0);

        #endregion

        #region Language Property

        //  language
        public static string LogInitialized { get; set; } = "[经验统计插件]初始化完成.";

        public static string TabHeader { get; set; } = "Rift Exp Analysis Plugin";
        public static string GreaterRift { get; set; } = "Greater Rift";
        public static string NephalemRift { get; set; } = "Nephalem Rift";
        public static string Avg { get; set; } = "Avg";
        public static string Max { get; set; } = "Max";
        public static string Min { get; set; } = "Min";
        public static string Total { get; set; } = "Total";
        public static string CostTime { get; set; } = "Cost Time";
        public static string Exp { get; set; } = "Exp";
        public static string CreateGame { get; set; } = "Create Game";
        public static string Death { get; set; } = "Death";
        public static string Count { get; set; } = "Count";
        public static string LevelUp { get; set; } = "Level Up";
        public static string RunningTime { get; set; } = "Running Time";
        public static string TotalExp { get; set; } = "Total Exp";
        public static string EveryHourExp { get; set; } = "Every Hour Exp";
        public static string LevelUpWith { get; set; } = "Level Up With {0}.";

        #endregion

        #region Func

        public static string GetFile(string startDirectory, string fileName)
        {
            return GetFile(startDirectory, new List<string> {fileName});
        }

        internal static string GetFile(string startDirectory, ICollection<string> fileNames)
        {
            var dirExcludes = new HashSet<string>
            {
                ".svn",
                "obj",
                "bin",
                "debug"
            };

            var queue = new Queue<string>();

            queue.Enqueue(startDirectory);

            Func<string, string> last = input => input.Split('\\').Last().ToLower();

            Func<IEnumerable<string>, string, bool> contains = (haystack, needle) =>
            {
                return haystack.Contains(needle, StringComparer.Create(Thread.CurrentThread.CurrentCulture, true));
            };

            while (queue.Count > 0)
            {
                startDirectory = queue.Dequeue();
                try
                {
                    foreach (var subDir in Directory.GetDirectories(startDirectory))
                    {
                        if (contains(dirExcludes, last(subDir))) continue;
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(startDirectory);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                    foreach (var filePath in files)
                    {
                        if (!contains(fileNames, last(filePath))) continue;
                        return filePath;
                    }
            }
            return string.Empty;
        }

        #endregion
    }
}