using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Common
{
    public static class WorkingDayHelper
    {
        private static List<DateTime> Holidays = new List<DateTime>();
        public static List<string> WorkingDay = new List<string>();

        //Holidays.Add(new DateTime(DateTime.Now.Year, 1, 1));
        //Holidays.Add(new DateTime(DateTime.Now.Year, 1, 5));
        //Holidays.Add(new DateTime(DateTime.Now.Year, 3, 10));
        //Holidays.Add(new DateTime(DateTime.Now.Year, 12, 25));

        public static void InitholidayDate(List<DateTime> holidaydates, List<string> WorkingDaylist)
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

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Sunday));
                    //return true;
                case DayOfWeek.Monday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Monday));
                // return false;
                case DayOfWeek.Tuesday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Tuesday));
                // return false;
                case DayOfWeek.Wednesday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Wednesday));
                //return false;
                case DayOfWeek.Thursday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Thursday));
                //return false;
                case DayOfWeek.Friday:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Friday));
                //return false; 
                default:
                    return !WorkingDay.Contains(Convert.ToString(DayOfWeek.Saturday));
                    // return true;                    
            }

            //return date.DayOfWeek == DayOfWeek.Saturday
             //   || date.DayOfWeek == DayOfWeek.Sunday;
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

        //public int GetWorkingDays(this DateTime current, DateTime finishDateExclusive, List<DateTime> excludedDates)
        //{
        //    Func<int, bool> isWorkingDay = days =>
        //    {
        //        var currentDate = current.AddDays(days);
        //        var isNonWorkingDay =
        //            currentDate.DayOfWeek == DayOfWeek.Saturday ||
        //            currentDate.DayOfWeek == DayOfWeek.Sunday ||
        //            excludedDates.Exists(excludedDate => excludedDate.Date.Equals(currentDate.Date));
        //        return !isNonWorkingDay;
        //    };

        //    return Enumerable.Range(0, (finishDateExclusive - current).Days).Count(isWorkingDay);
        //}

        //public static DateTime AddDays(DateTime from, int days)
        //{
        //    int start = (int)from.DayOfWeek;
        //    int end = (start + days);
        //    int weeks = end / 7;
        //    end = end + weeks;
        //    DateTime result = from.AddDays(end - start);
        //    if (result.DayOfWeek == DayOfWeek.Saturday)
        //    {
        //        result = result.AddDays(2);
        //    }
        //    else if (result.DayOfWeek == DayOfWeek.Sunday)
        //    {
        //        result = result.AddDays(1);
        //    }
        //    return result;
        //}

        //public static DateTime AddMinuesDay(DateTime from, int days)
        //{
        //    int start = (int)from.DayOfWeek;
        //    int end = (start + days);
        //    int weeks = end / 7;
        //    end = end + weeks;
        //    DateTime result = from.AddDays(end - start);
        //    if (result.DayOfWeek == DayOfWeek.Saturday)
        //    {
        //        result = result.AddDays(-1);
        //    }
        //    else if (result.DayOfWeek == DayOfWeek.Sunday)
        //    {
        //        result = result.AddDays(-2);
        //    }
        //    return result;
        //}

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
                //while (current.DayOfWeek == DayOfWeek.Saturday ||
                //    current.DayOfWeek == DayOfWeek.Sunday);
                while (IsHoliday(current) || IsWeekend(current));
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
    }
}
