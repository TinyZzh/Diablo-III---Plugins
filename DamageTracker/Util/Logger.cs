using Zeta.Common;

namespace StatsTracker.Util
{
    internal static class Logger
    {
        private static readonly log4net.ILog DbLog = Zeta.Common.Logger.GetLoggerInstanceForType();

        internal static void Error(string message, params object[] args)
        {
            message = "[" + Local.LocalPluginName + "] " + message;
            DbLog.ErrorFormat(message, args);
        }

        internal static void Info(string message, params object[] args)
        {
            message = "[" + Local.LocalPluginName + "] " + message;
            DbLog.InfoFormat(message, args);
        }

        internal static void Debug(string message, params object[] args)
        {
            message = "[" + Local.LocalPluginName + "] " + message;
            DbLog.DebugFormat(message, args);
        }

        internal static void Verbose(string message)
        {
            message = "[" + Local.LocalPluginName + "] " + message;
            DbLog.Verbose(message);
        }
    }

}
