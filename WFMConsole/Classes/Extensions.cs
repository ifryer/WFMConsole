using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentDateTime;

namespace WFMConsole.Classes
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime GetNextWeekday(DateTime date, List<DayOfWeek> days)
        {
            DateTime result = date.AddDays(1);
            while (!days.Contains(result.DayOfWeek))
                result = result.AddDays(1);
            return result;
        }

        public static List<DateTime> GetSpecifiedDaysFromDate(DateTime date, List<DayOfWeek> days, int repeatCount, int repeatEveryWeeks)
        {
            List<DateTime> results = new List<DateTime>();
            if (repeatEveryWeeks <= 1)
            {
                date = date.AddDays(1); // Do this so it doesn't include the date we selected
                results = Enumerable.Range(0, repeatCount * 7).Select(d => date.AddDays(d)).Where(d => days.Contains(d.DayOfWeek)).Take(repeatCount).ToList();
            }
            else
            {
                var workingDate = date;
                var endOfWorkingWeek = FluentDateTime.DateTimeExtensions.LastDayOfWeek(workingDate);

                for (int i = 0; i < repeatCount; i++)
                {
                    workingDate = WFMConsole.Classes.DateTimeExtensions.GetNextWeekday(workingDate,  days );
                    if(workingDate > endOfWorkingWeek)
                    {
                        //Don't add it, skip to the next valid week and add that day, then continue
                        workingDate = workingDate.AddDays(7 * (repeatEveryWeeks - 1));
                        endOfWorkingWeek = FluentDateTime.DateTimeExtensions.LastDayOfWeek(workingDate);
                    }
                    results.Add(workingDate);
                }


                //Get the rest of the repeat days on this week
                //Increment startIndex with each one

                //i = startIndex
                //While i < repeatCount
                // go to first day of next week to repeat on
                // Add (repeatEveryWeeks-1) weeks
                //Get the rest of the repeat days on this week, as long as i < repeatcount
                //Increment i with each one
            }
            return results;
        }
    }
}