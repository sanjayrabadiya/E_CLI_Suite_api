using System;

namespace GSC.Shared.Extension
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset,
            DateTimeOffset? dateOfDeath)
        {
            var dateToCalculateTo = DateTime.UtcNow;

            if (dateOfDeath != null) dateToCalculateTo = dateOfDeath.Value.UtcDateTime;

            var age = dateToCalculateTo.Year - dateTimeOffset.Year;

            if (dateToCalculateTo < dateTimeOffset.AddYears(age)) age--;

            return age;
        }

        public static DateTime UtcDate(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dateTime.ToUniversalTime();
        }

        public static DateTime? UtcDate(this DateTime? dateTime)
        {
            if (dateTime?.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(Convert.ToDateTime(dateTime), DateTimeKind.Utc);
            return dateTime?.ToUniversalTime();
        }
    }
}