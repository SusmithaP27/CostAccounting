using System;
using System.Threading;
using System.Threading.Tasks;
using CostAccounting.Services.EmployeeRateService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CostAccounting.Services
{
    // Runs continuously in the background and, once a day, checks whether today is the
    // seasonal rate-adjustment date (first Sunday after Thanksgiving = +10%, last Saturday
    // in March = -10%). If so, it applies the adjustment to every active employee with a
    // current rate — no manual button click required. The "Apply Seasonal Adjustment" button
    // on the Employee Rates page still exists as a manual override (e.g. to re-run for a
    // subset of employees, or on a different date), and ApplySeasonalAdjustmentAsync's own
    // same-day idempotency check means this service and a manual click can't double-apply.
    public class SeasonalRateAdjustmentBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SeasonalRateAdjustmentBackgroundService> _logger;

        // How often to check. Daily would technically be enough, but checking every few hours
        // means a mid-day app restart (deploy, recycle, etc.) doesn't cause the day's
        // adjustment to be missed entirely.
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

        public SeasonalRateAdjustmentBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SeasonalRateAdjustmentBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndApplyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Seasonal rate adjustment check failed.");
                }

                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected on shutdown.
                }
            }
        }

        private async Task CheckAndApplyAsync(CancellationToken stoppingToken)
        {
            var today = DateTime.Today;
            var increaseDate = SeasonalRateHelper.GetFirstSundayAfterThanksgiving(today.Year);
            var decreaseDate = SeasonalRateHelper.GetLastSaturdayInMarch(today.Year);

            bool isIncreaseDay = today == increaseDate;
            bool isDecreaseDay = today == decreaseDate;
            if (!isIncreaseDay && !isDecreaseDay)
                return; // most days, nothing to do

            using var scope = _scopeFactory.CreateScope();
            var rateService = scope.ServiceProvider.GetRequiredService<IEmployeeRateService>();

            const string systemUser = "System (Seasonal Auto-Adjust)";

            var (success, message) = await rateService.ApplySeasonalAdjustmentAsync(
                isIncreaseDay, today, employeeObjectIds: null, enteredByUser: systemUser);

            _logger.LogInformation(
                "Automatic seasonal rate adjustment for {Date} ({Direction}): success={Success} — {Message}",
                today, isIncreaseDay ? "increase" : "decrease", success, message);
        }
    }
}
