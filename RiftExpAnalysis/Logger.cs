using log4net;
using Zeta.Common;

namespace TinyZ.RiftExpAnalysis
{
    internal static class Logger
    {
        private static readonly ILog DbLog = Zeta.Common.Logger.GetLoggerInstanceForType();

        internal static void Error(string message, params object[] args)
        {
            message = "[" + RiftExpAnalysisPugin.NAME + " V" + RiftExpAnalysisPugin.VERSION + "]" + message;
            DbLog.ErrorFormat(message, args);
        }

        internal static void Info(string message, params object[] args)
        {
            message = "[" + RiftExpAnalysisPugin.NAME + " V" + RiftExpAnalysisPugin.VERSION + "]" + message;
            DbLog.InfoFormat(message, args);
        }

        internal static void Debug(string message, params object[] args)
        {
            message = "[" + RiftExpAnalysisPugin.NAME + " V" + RiftExpAnalysisPugin.VERSION + "]" + message;
            DbLog.DebugFormat(message, args);
        }

        internal static void Verbase(string message)
        {
            message = "[" + RiftExpAnalysisPugin.NAME + " V" + RiftExpAnalysisPugin.VERSION + "]" + message;
            DbLog.Verbose(message);
        }

        internal static void Warn(string message)
        {
            message = "[" + RiftExpAnalysisPugin.NAME + " V" + RiftExpAnalysisPugin.VERSION + "]" + message;
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