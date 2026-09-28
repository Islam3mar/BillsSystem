using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Common
{
    // توقيت مصر ثابت بغض النظر عن توقيت السيرفر (Azure/Docker غالبًا UTC)
    public static class AppClock
    {
        private static readonly TimeZoneInfo Zone = Resolve();

        private static TimeZoneInfo Resolve()
        {
            foreach (var id in new[] { "Africa/Cairo", "Egypt Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            throw new InvalidOperationException(
                "Cairo time zone not found. On Linux/Docker install the 'tzdata' package.");
        }

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
        public static DateTime Today => Now.Date;
    }
}
