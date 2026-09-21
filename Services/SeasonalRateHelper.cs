using System;

namespace CostAccounting.Services
{
    // Two fixed annual dates that drive the seasonal rate adjustment:
    //   - Rates go UP 10% on the first Sunday after Thanksgiving
    //   - Rates go back DOWN 10% on the last Saturday in March
    public static class SeasonalRateHelper
    {
        public static DateTime GetFirstSundayAfterThanksgiving(int year)
        {
            // Thanksgiving = 4th Thursday of November
            var nov1 = new DateTime(year, 11, 1);
            int daysUntilFirstThursday = ((int)DayOfWeek.Thursday - (int)nov1.DayOfWeek + 7) % 7;
            var firstThursday = nov1.AddDays(daysUntilFirstThursday);
            var thanksgiving = firstThursday.AddDays(21); // 4th Thursday

            // Thursday -> following Sunday is always +3 days
            return thanksgiving.AddDays(3);
        }

        public static DateTime GetLastSaturdayInMarch(int year)
        {
            var mar31 = new DateTime(year, 3, 31);
            int diff = ((int)mar31.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
            return mar31.AddDays(-diff);
        }

        // Whichever of the two seasonal dates comes next from "today" (used to pre-fill the
        // Apply Seasonal Adjustment modal with a sensible default).
        public static (DateTime IncreaseDate, DateTime DecreaseDate, bool NextIsIncrease, DateTime NextDate) GetUpcoming(DateTime today)
        {
            var increaseThisYear = GetFirstSundayAfterThanksgiving(today.Year);
            var decreaseThisYear = GetLastSaturdayInMarch(today.Year);

            // Build the next occurrence of each, rolling to next year if this year's has passed.
            var nextIncrease = increaseThisYear >= today.Date ? increaseThisYear : GetFirstSundayAfterThanksgiving(today.Year + 1);
            var nextDecrease = decreaseThisYear >= today.Date ? decreaseThisYear : GetLastSaturdayInMarch(today.Year + 1);

            bool nextIsIncrease = nextIncrease <= nextDecrease;
            var nextDate = nextIsIncrease ? nextIncrease : nextDecrease;

            return (nextIncrease, nextDecrease, nextIsIncrease, nextDate);
        }
    }
}
