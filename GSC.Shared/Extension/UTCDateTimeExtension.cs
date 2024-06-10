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

        public static DateTime GetFirstDateOfMonth(this DateTime dateTime)
        {
            return  new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime GetLastDateOfMonth(this DateTime dateTime)
        {
            var startDate = new DateTime(dateTime.Year, dateTime.Month, 1);
            return startDate.AddMonths(1).AddDays(-1);
        }
    }
}