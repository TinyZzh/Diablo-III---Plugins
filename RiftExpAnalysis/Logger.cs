using log4net;
using Zeta.Common;

namespace TinyZ.RiftExpAnalysis
{
    internal static class Logger
    {
        private static readonly ILog DbLog = Zeta.Common.Logger.GetLoggerInstanceForType();

        public static void Error(string message, params object[] args)
        {
            message = "[" + Localize.PluginName + " V" + Localize.PluginVersison + "]" + message;
            DbLog.ErrorFormat(message, args);
        }

        public static void Info(string message, params object[] args)
        {
            message = "[" + Localize.PluginName + " V" + Localize.PluginVersison + "]" + message;
            DbLog.InfoFormat(message, args);
        }

        public static void Debug(string message, params object[] args)
        {
            message = "[" + Localize.PluginName + " V" + Localize.PluginVersison + "]" + message;
            DbLog.DebugFormat(message, args);
        }

        public static void Verbase(string message)
        {
            message = "[" + Localize.PluginName + " V" + Localize.PluginVersison + "]" + message;
            DbLog.Verbose(message);
        }

        public static void Warn(string message)
        {
            message = "[" + Localize.PluginName + " V" + Localize.PluginVersison + "]" + message;
            DbLog.WarnFormat(message);
        }

        internal static void System(string message)
        {
            Warn("[插件信息]" + message);
        }

        internal static void Count(string message)
        {
            Info("[经验统计]" + message);
        }
    }
}