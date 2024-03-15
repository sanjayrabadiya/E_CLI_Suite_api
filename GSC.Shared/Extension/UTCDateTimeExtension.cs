using System;

namespace GSC.Shared.Extension
{
    public static class UtcDateTimeExtension
    {
        public static DateTime UtcDateTime(this DateTime dateTime)
        {
            return dateTime;
        }

        public static DateTime? UtcDateTime(this DateTime? dt)
        {
            return dt;
        }

        public static DateTime UtcDate(this DateTime dateTime)
        {
            return dateTime;
        }

        public static DateTime? UtcDate(this DateTime? dateTime)
        {
            return dateTime;
        }
    }
}