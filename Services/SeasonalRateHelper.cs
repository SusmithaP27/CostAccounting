using System;

namespace CostAccounting.Helpers
{
    public static class SeasonalRateHelper
    {
        // Thanksgiving = 4th Thursday of November. The Sunday after it is Thanksgiving + 3 days.
        public static DateTime FirstSundayAfterThanksgiving(int year)
        {
            var d = new DateTime(year, 11, 1);
            while (d.DayOfWeek != DayOfWeek.Thursday) d = d.AddDays(1); // 1st Thursday
            return d.AddDays(21 + 3);                                   // 4th Thursday + 3 = Sunday
        }

        public static DateTime LastSaturdayInMarch(int year)
        {
            var d = new DateTime(year, 3, 31);
            while (d.DayOfWeek != DayOfWeek.Saturday) d = d.AddDays(-1);
            return d;
        }

        // Next occurrence of each date (today counts), and which one comes first.
        public static (DateTime IncreaseDate, DateTime DecreaseDate, bool NextIsIncrease, DateTime NextDate)
            GetUpcoming(DateTime today)
        {
            today = today.Date;

            var increase = FirstSundayAfterThanksgiving(today.Year);
            if (increase < today) increase = FirstSundayAfterThanksgiving(today.Year + 1);

            var decrease = LastSaturdayInMarch(today.Year);
            if (decrease < today) decrease = LastSaturdayInMarch(today.Year + 1);

            var nextIsIncrease = increase <= decrease;
            return (increase, decrease, nextIsIncrease, nextIsIncrease ? increase : decrease);
        }

        // Increase period: first Sunday after Thanksgiving -> day before the last Saturday in March
        public static (DateTime Start, DateTime End) GetIncreasePeriod(DateTime today)
        {
            today = today.Date;
            var start = FirstSundayAfterThanksgiving(today.Year);
            if (start < today) start = FirstSundayAfterThanksgiving(today.Year + 1);
            return (start, LastSaturdayInMarch(start.Year + 1).AddDays(-1));
        }

        // Decrease period: last Saturday in March -> day before the first Sunday after Thanksgiving
        public static (DateTime Start, DateTime End) GetDecreasePeriod(DateTime today)
        {
            today = today.Date;
            var start = LastSaturdayInMarch(today.Year);
            if (start < today) start = LastSaturdayInMarch(today.Year + 1);
            return (start, FirstSundayAfterThanksgiving(start.Year).AddDays(-1));
        }
    }
}
