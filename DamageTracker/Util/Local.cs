using System;
using System.Collections.Generic;

namespace StatsTracker.Util
{
    /// <summary>
    ///     本地化语言
    /// </summary>
    internal class Local
    {
        /// <summary>
        ///     对应语言文件
        /// </summary>
        public static string Language = "zh-CN";

        public static Version PluginVersion = new Version(1, 0, 0);

        // 
        public static string LocalPluginName = "DPS统计";
        public static string LocalPluginTabToolTip = "伤害收集统计插件";

        public static string LocalPluginDescription = "伤害统计插件 v" + PluginVersion;
        public static string LocalTotalDamage = "累计总伤害量";
        public static string LocalTotalDamage10Sec = "累计伤害(10秒)";
        public static string LocalAverageDps = "平均DPS";
        public static string LocalMaxDps = "最高DPS";
        public static string LocalAverageDps10Sec = "平均DPS(10秒)";
        public static string LocalCurrentDps = "DPS";
        public static string LocalResetDamageStats = "重置";


        /// <summary>
        ///     显示伤害数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ShowDamage(float value)
        {
            switch (Language)
            {
                case "zh-CN":
                    return ShowDamageByZhCn(value);
                default:
                    return $"{value:N0}";
            }
        }

        /// <summary>
        ///     根据Zh-CN习惯显示伤害数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ShowDamageByZhCn(float value)
        {
            var pairs = new[]
            {
                new KeyValuePair<float, string>(1000000000000f, "万亿"),
                new KeyValuePair<float, string>(100000000f, "亿"),
                new KeyValuePair<float, string>(10000000f, "千万"),
                new KeyValuePair<float, string>(1000000f, "百万"),
                new KeyValuePair<float, string>(10000f, "万")
            };
            foreach (var pair in pairs)
            {
                if (value >= pair.Key)
                    return $"{(value / pair.Key):f2}" + pair.Value;
            }
            return value.ToString();
        }
    }
}