namespace Kitakun.VkModules.Core
{
    using System;

    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime dt) =>
            new DateTime(dt.Year, dt.Month, 1);

        public static DateTime LastDayOfMonth(this DateTime dt) =>
            new DateTime(dt.Year, dt.Month, 1)
                .AddMonths(1)
                .AddSeconds(-1);
    }
}
