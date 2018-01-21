using System;

namespace YouRock
{
    public class DateHelper
    {
        public enum TimePeriod
        {
            QuarterHour = 1,
            HalfHour = 2,
            Hour = 3,
            Day = 4,
            Week = 5,
            Month = 6,
            Year = 7,
        }
        
        public static Tuple<DateTime, DateTime> CalculateStartEndDate(DateTime date, TimePeriod timePeriod, bool nextPeriod = false)
        {
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();

            if (timePeriod == TimePeriod.QuarterHour)
            {
                if (date.Minute < 15)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                }
                else if (date.Minute > 14 && date.Minute < 30)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 15, 0);
                }
                else if (date.Minute > 29 && date.Minute < 45)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 30, 0);
                }
                else if (date.Minute > 44)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 45, 0);
                }

                if (nextPeriod)
                {
                    startDate = startDate.AddMinutes(15);
                }

                endDate = startDate.AddMinutes(15).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.HalfHour)
            {
                if (date.Minute < 30)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                }
                else if (date.Minute > 29)
                {
                    startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 30, 0);
                }

                if (nextPeriod)
                {
                    startDate = startDate.AddMinutes(30);
                }

                endDate = startDate.AddMinutes(30).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.Hour)
            {
                startDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                
                if (nextPeriod)
                {
                    startDate = startDate.AddHours(1);
                }

                endDate = startDate.AddHours(1).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.Day)
            {
                startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

                if (nextPeriod)
                {
                    startDate = startDate.AddDays(1);
                }

                endDate = startDate.AddDays(1).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.Week)
            {
                startDate = DateHelper.StartOfWeek(date, DayOfWeek.Monday);
                
                if (nextPeriod)
                {
                    startDate = startDate.AddDays(7);
                }

                endDate = startDate.AddDays(7).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.Month)
            {
                startDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0);

                if (nextPeriod)
                {
                    startDate = startDate.AddMonths(1);
                }

                endDate = startDate.AddMonths(1).AddMilliseconds(-1);
            }
            else if (timePeriod == TimePeriod.Year)
            {
                startDate = new DateTime(date.Year, 1, 1, 0, 0, 0);

                if (nextPeriod)
                {
                    startDate = startDate.AddYears(1);
                }

                endDate = startDate.AddYears(1).AddMilliseconds(-1);
            }

            return new Tuple<DateTime, DateTime>(startDate, endDate);
        }

        public static long ShortDate(DateTime date)
        {
            return Convert.ToInt64(date.ToString("yyyyMMddHHmm"));
        }

        public static DateTime StartOfWeek(DateTime datetime, DayOfWeek startOfWeek)
        {
            int diff = (7 + (datetime.DayOfWeek - startOfWeek)) % 7;
            return datetime.AddDays(-1 * diff).Date;
        }
    }
}