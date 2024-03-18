using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Common
{
    public static class WorkingDayHelper
    {
        private static List<DateTime> Holidays = new List<DateTime>();
        public static List<WeekendData> WorkingDay = new List<WeekendData>();

        public static void InitholidayDate(List<DateTime> holidaydates, List<WeekendData> WorkingDaylist)
        {
            Holidays = holidaydates;
            WorkingDay = WorkingDaylist;
        }

        private static bool IsHoliday(DateTime date)
        {
            return Holidays.Contains(new DateTime(date.Year, date.Month, date.Day));
        }

        private static bool IsWeekend(DateTime date)
        {
            if (WorkingDay.Exists(x => x.Weekend == date.DayOfWeek.ToString()))
            {
                var result = WorkingDay.Find(x => x.Weekend == date.DayOfWeek.ToString());
                if (result != null)
                {
                    if (result.Frequency == "All")
                    {
                        return true;
                    }
                    else
                    {
                        return Frequency(date, result.Frequency);
                    }
                }

            }

            return false;
        }
        public static DateTime GetNextWorkingDay(DateTime date)
        {
            do
            {
                date = date.AddDays(1);
            } while (IsHoliday(date) || IsWeekend(date));
            return date;
        }

        public static DateTime GetNextSubstarctWorkingDay(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            } while (IsHoliday(date) || IsWeekend(date));
            return date;
        }

        public static DateTime AddBusinessDays(this DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                }
                while (IsHoliday(current) || IsWeekend(current));
                //while (IsHoliday(current));
            }
            return current;
        }
        public static DateTime SubtractBusinessDays(this DateTime current, int days)
        {
            return AddBusinessDays(current, -days);
        }


        public static List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> allDates = new List<DateTime>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                allDates.Add(date.Date);
            }

            return allDates;
        }

        public class WeekendData
        {
            public string Weekend { get; set; }
            public string Frequency { get; set; }
        }

        public static bool Frequency(DateTime date, string frequency)
        {
            int WeekNumber = GetWeekNumberOfMonth(date);
            List<int> data = new List<int>();
            if (frequency == "Odd(1,3,5)")
            {
                data = new List<int> { 1, 3, 5 };
            }
            else if (frequency == "Even(2,4)")
            {
                data = new List<int> { 2, 4 };
            }
            else if (frequency == "1st")
            {
                data = new List<int> { 1 };
            }
            else if (frequency == "2nd")
            {
                data = new List<int> { 2 };
            }
            else if (frequency == "3rd")
            {
                data = new List<int> { 3 };
            }
            else if (frequency == "4th")
            {
                data = new List<int> { 4 };
            }
            else if (frequency == "5th")
            {
                data = new List<int> { 5 };
            }

            return data.Contains(WeekNumber);
        }

        public static int GetWeekNumberOfMonth(DateTime date)
        {
            decimal numberofday = date.Day;
            decimal d = (Math.Floor(numberofday / 7)) + 1;

            if ((numberofday) % 7 == 0)
            {
                return Convert.ToInt32((d)) - 1;
            }
            return Convert.ToInt32(d);

        }
    }
}
