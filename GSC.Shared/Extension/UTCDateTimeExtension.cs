using System;

namespace GSC.Shared.Extension
{
    public static class UtcDateTimeExtension
    {
        public static DateTime UtcDateTime(this DateTime dateTime)
        {
            //if (dateTime.Kind == DateTimeKind.Unspecified)
            //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            //return dateTime.ToUniversalTime();

            return dateTime;
        }

        public static DateTime? UtcDateTime(this DateTime? dt)
        {
            //if (dt?.Kind == DateTimeKind.Unspecified)
            //    dt = DateTime.SpecifyKind(Convert.ToDateTime(dt), DateTimeKind.Utc);
            //return dt?.ToUniversalTime();

            return dt;
        }

        public static DateTime UtcDate(this DateTime dateTime)
        {
            //if (dateTime.Kind == DateTimeKind.Unspecified)
            //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            //return dateTime.ToUniversalTime();
            return dateTime;
        }

        public static DateTime? UtcDate(this DateTime? dateTime)
        {
            //if (dateTime?.Kind == DateTimeKind.Unspecified)
            //    dateTime = DateTime.SpecifyKind(Convert.ToDateTime(dateTime), DateTimeKind.Utc);
            //return dateTime?.ToUniversalTime();

            return dateTime;
        }
    }
}