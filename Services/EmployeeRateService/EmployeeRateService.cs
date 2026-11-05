using Microsoft.EntityFrameworkCore;
using CostAccounting.Models;
using CostAccounting.Data;


namespace CostAccounting.Services.EmployeeRateService
{
    public class EmployeeRateService : IEmployeeRateService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeRateIndexVM> GetIndexAsync(EmployeeRateIndexVM filter)
        {
            var query = _context.EmployeeRates
                .Include(er => er.Employee)
                .Where(er => !er.Deleted && !er.Employee.Deleted)
                .AsQueryable();

            // if (!filter.ShowInactiveEmployees)
            //     query = query.Where(er => er.Employee.Active);

            if (filter.EmployeeObjectId.HasValue)
                query = query.Where(er => er.EmployeeObjectId == filter.EmployeeObjectId);

            // Status filter (same rules as EmployeeRateVM.IsCurrent / IsScheduled):
            //   current   : start date <= today AND (no end date OR end date >= today)   [default]
            //   scheduled : start date > today
            //   ended     : end date < today
            //   all       : no status filtering
            var today = DateTime.Today;
            switch ((filter.Status ?? "current").ToLowerInvariant())
            {
                case "all":
                    break;
                case "scheduled":
                    query = query.Where(er => er.StartDate > today);
                    break;
                case "ended":
                    query = query.Where(er => er.EndDate != null && er.EndDate < today);
                    break;
                default: // "current"
                    query = query.Where(er => er.StartDate <= today && (er.EndDate == null || er.EndDate >= today));
                    break;
            }

            if (filter.DateRangeStart.HasValue)
                query = query.Where(er => er.StartDate >= filter.DateRangeStart);

            if (filter.DateRangeEnd.HasValue)
                query = query.Where(er => er.StartDate <= filter.DateRangeEnd);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(er =>
                    er.Employee.FirstName.Contains(term) ||
                    er.Employee.LastName.Contains(term) ||
                    er.Employee.Code.Contains(term));
            }

            filter.TotalCount = await query.CountAsync();

            // Employee name sorts by last name, then first name, then newest rate first, so two
            // people with the same last name stay in first-name order and one person's rows
            // stay together. Every sort ends with ObjectID so ties keep a stable order across pages.
            var asc = filter.SortDirection == "asc";
            IOrderedQueryable<EmployeeRate> ordered = filter.SortField switch
            {
                "EmployeeName" => asc
                    ? query.OrderBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName)
                    : query.OrderByDescending(er => er.Employee.LastName).ThenByDescending(er => er.Employee.FirstName),
                "Rate" => asc
                    ? query.OrderBy(er => er.Rate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName)
                    : query.OrderByDescending(er => er.Rate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName),
                "EndDate" => asc
                    ? query.OrderBy(er => er.EndDate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName)
                    : query.OrderByDescending(er => er.EndDate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName),
                _ => asc
                    ? query.OrderBy(er => er.StartDate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName)
                    : query.OrderByDescending(er => er.StartDate).ThenBy(er => er.Employee.LastName).ThenBy(er => er.Employee.FirstName),
            };

            if (filter.SortField == "EmployeeName")
                ordered = ordered.ThenByDescending(er => er.StartDate);

            query = ordered.ThenBy(er => er.ObjectID);

            var page = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(er => new EmployeeRateVM
                {
                    ObjectID = er.ObjectID,
                    EmployeeObjectId = er.EmployeeObjectId,
                    EmployeeName = er.Employee.FirstName + " " + er.Employee.LastName,
                    EmployeeCode = er.Employee.Code,
                    StartDate = er.StartDate,
                    EndDate = er.EndDate,
                    Rate = er.Rate,
                    BaseRate = er.BaseRate
                })
                .ToListAsync();

            filter.EmployeeRates = page;
            return filter;
        }

        public async Task<EmployeeRateVM> GetByIdAsync(int objectId)
        {
            var er = await _context.EmployeeRates
                .Include(x => x.Employee)
                .Where(x => x.ObjectID == objectId && !x.Deleted)
                .FirstOrDefaultAsync();

            if (er == null) return null;

            return new EmployeeRateVM
            {
                ObjectID = er.ObjectID,
                EmployeeObjectId = er.EmployeeObjectId,
                EmployeeName = er.Employee.LastName + ", " + er.Employee.FirstName,
                EmployeeCode = er.Employee.Code,
                StartDate = er.StartDate,
                EndDate = er.EndDate,
                Rate = er.Rate,
                BaseRate = er.BaseRate
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(EmployeeRateVM vm, string enteredByUser)
        {
            var entity = new EmployeeRate
            {
                EmployeeObjectId = vm.EmployeeObjectId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Rate = vm.Rate,
                BaseRate = vm.BaseRate,
                EnteredByUser = enteredByUser,
                EnteredDate = DateTime.Now,
                Deleted = false
            };

            _context.EmployeeRates.Add(entity);
            await _context.SaveChangesAsync();
            return (true, "Employee rate created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(EmployeeRateVM vm, string enteredByUser)
        {
            var entity = await _context.EmployeeRates.FindAsync(vm.ObjectID);
            if (entity == null || entity.Deleted)
                return (false, "Employee rate not found.");

            entity.EmployeeObjectId = vm.EmployeeObjectId;
            entity.StartDate = vm.StartDate;
            entity.EndDate = vm.EndDate;
            entity.Rate = vm.Rate;
            entity.BaseRate = vm.BaseRate;
            entity.EnteredByUser = enteredByUser;
            entity.EnteredDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Employee rate updated successfully.");
        }

        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var entity = await _context.EmployeeRates.FindAsync(objectId);
            if (entity == null)
                return (false, "Employee rate not found.");

            entity.Deleted = true;
            await _context.SaveChangesAsync();
            return (true, "Employee rate deleted successfully.");
        }

        // Uses FindAsync per ID rather than a Contains() bulk query — same workaround
        // used on EquipmentRate to avoid EF Core's Contains() -> CTE translation issue.
        public async Task<(bool Success, string Message)> BulkEndDateAsync(List<int> objectIds, DateTime endDate)
        {
            if (objectIds == null || !objectIds.Any())
                return (false, "No rates selected.");

            foreach (var id in objectIds)
            {
                var entity = await _context.EmployeeRates.FindAsync(id);
                if (entity != null && !entity.Deleted)
                    entity.EndDate = endDate;
            }

            await _context.SaveChangesAsync();
            return (true, $"{objectIds.Count} employee rate(s) updated.");
        }

        // Seasonal : multiplies Rate only; the new row runs startDate..endDate.
        // Annual   : multiplies Rate AND BaseRate; the new row starts on startDate and is open-ended.
        // In both cases the rate in effect the day before startDate is end-dated that day, and the
        // new row is added. Whether it shows as Current/Scheduled/Ended is decided by its dates
        // (see EmployeeRateVM.IsCurrent / IsScheduled), not by this method.
        //   percent      : 10 = the standard seasonal swing; any other value = custom percentage.
        //   selectedOnly : true = only employeeObjectIds are adjusted (never falls back to everyone).
        // Increase and decrease are independent moves (not a perfect round-trip: 1.10 * 0.90 = 0.99).
        public async Task<(bool Success, string Message)> ApplySeasonalAdjustmentAsync(
            bool isSeasonal, bool isIncrease, DateTime startDate, DateTime? endDate,
            List<int> employeeObjectIds, bool selectedOnly, decimal percent, string enteredByUser)
        {
            if (percent <= 0 || percent > 100 || (!isIncrease && percent >= 100))
                return (false, "Percentage must be greater than 0 (and less than 100 for a decrease).");

            if (selectedOnly && (employeeObjectIds == null || !employeeObjectIds.Any()))
                return (false, "No employees were selected.");

            startDate = startDate.Date;
            if (isSeasonal)
            {
                if (!endDate.HasValue)
                    return (false, "A seasonal adjustment requires an end date.");
                if (endDate.Value.Date < startDate)
                    return (false, "End date must be on or after the start date.");
            }
            DateTime? newEndDate = isSeasonal ? endDate.Value.Date : (DateTime?)null;
            var dayBefore = startDate.AddDays(-1);
            var multiplier = isIncrease ? 1m + percent / 100m : 1m - percent / 100m;

            // Same anti-Contains() workaround as before: fetch broadly, filter in memory.
            var candidates = await _context.EmployeeRates
                .Where(er => !er.Deleted && (er.EndDate == null || er.EndDate >= dayBefore))
                .Include(er => er.Employee)
                .ToListAsync();

            var targetSet = selectedOnly ? new HashSet<int>(employeeObjectIds) : null; // null = everyone active

            var eligible = candidates
                .Where(er => !er.Employee.Deleted && er.Employee.Active)
                .Where(er => targetSet == null || targetSet.Contains(er.EmployeeObjectId))
                .ToList();

            // Idempotency guard: anyone who already has a rate starting on startDate was adjusted
            // already (earlier run, manual re-click, or the daily background check) — skip them.
            var alreadyApplied = eligible
                .Where(er => er.StartDate.Date == startDate)
                .Select(er => er.EmployeeObjectId)
                .ToHashSet();

            // Source rate = the rate in effect the day before the new one starts.
            var sources = eligible
                .Where(er => !alreadyApplied.Contains(er.EmployeeObjectId))
                .Where(er => er.StartDate.Date <= dayBefore &&
                             (er.EndDate == null || er.EndDate.Value.Date >= dayBefore))
                .GroupBy(er => er.EmployeeObjectId)
                .Select(g => g.OrderByDescending(er => er.StartDate).First())
                .ToList();

            var label = isSeasonal ? "Seasonal" : "Annual";

            if (!sources.Any())
                return (true, $"No employees required a {label.ToLower()} adjustment for {startDate:MM/dd/yyyy} (already applied, or no eligible employees).");

            foreach (var oldRate in sources)
            {
                oldRate.EndDate = dayBefore;

                _context.EmployeeRates.Add(new EmployeeRate
                {
                    EmployeeObjectId = oldRate.EmployeeObjectId,
                    StartDate = startDate,
                    EndDate = newEndDate,
                    Rate = Math.Round(oldRate.Rate * multiplier, 2),
                    BaseRate = isSeasonal
                        ? oldRate.BaseRate
                        : (oldRate.BaseRate.HasValue
                            ? Math.Round(oldRate.BaseRate.Value * multiplier, 2)
                            : (decimal?)null),
                    EnteredByUser = enteredByUser,
                    EnteredDate = DateTime.Now,
                    Deleted = false
                });
            }

            await _context.SaveChangesAsync();

            var direction = isIncrease ? "increased" : "decreased";
            var what = isSeasonal ? "Rates" : "Rates and base rates";
            var range = isSeasonal
                ? $"{startDate:MM/dd/yyyy} – {newEndDate:MM/dd/yyyy}"
                : $"effective {startDate:MM/dd/yyyy}";
            return (true, $"{label}: {what} {direction} {percent:0.##}% for {sources.Count} employee(s), {range}.");
        }
    }
}
