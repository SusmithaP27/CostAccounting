using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.MaterialRateService
{
    public class MaterialRateService : IMaterialRateService
    {
        private readonly ApplicationDbContext _context;

        public MaterialRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<MaterialRateVM> Items, int TotalCount)> GetRatesAsync(
    int? filterMaterialId,
    bool showAllMaterials,
    string? startDateOperator,
    DateTime? startDateFilter,
    string? endDateOperator,
    DateTime? endDateFilter,
    bool showAllDates,
    string? searchTerm,
    string? sortColumn,
    string? sortDir,
    int page,
    int pageSize)
{
    var query =
        from r in _context.MaterialsRate
        where r.Deleted != true
        join m in _context.Materials on r.MaterialObjectId equals m.ObjectID
        select new MaterialRateVM
        {
            ObjectID = r.ObjectID,
            MaterialObjectId = r.MaterialObjectId,
            MaterialCode = m.Code,
            MaterialDescription = m.Description,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Rate = r.Rate
        };

    // ── Material filter ──────────────────────────────────────────────
    if (!showAllMaterials && filterMaterialId.HasValue)
        query = query.Where(x => x.MaterialObjectId == filterMaterialId.Value);

    // ── Date filters (each side independent) ─────────────────────────
    // Compared at day granularity so a stored time part can't break "=".
    if (!showAllDates)
    {
        if (startDateFilter.HasValue)
        {
            var d = startDateFilter.Value.Date;
            var next = d.AddDays(1);
            query = (startDateOperator ?? "<=") switch
            {
                "<"  => query.Where(x => x.StartDate < d),
                ">"  => query.Where(x => x.StartDate >= next),
                ">=" => query.Where(x => x.StartDate >= d),
                "="  => query.Where(x => x.StartDate >= d && x.StartDate < next),
                _    => query.Where(x => x.StartDate < next),   // "<="
            };
        }

        if (endDateFilter.HasValue)
        {
            var d = endDateFilter.Value.Date;
            var next = d.AddDays(1);
            query = (endDateOperator ?? ">=") switch
            {
                "<"  => query.Where(x => x.EndDate < d),
                "<=" => query.Where(x => x.EndDate < next),
                ">"  => query.Where(x => x.EndDate >= next),
                "="  => query.Where(x => x.EndDate >= d && x.EndDate < next),
                _    => query.Where(x => x.EndDate >= d),       // ">="
            };
        }
    }

    // ── Search ───────────────────────────────────────────────────────
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.Trim();
        query = query.Where(x =>
            (x.MaterialCode != null && EF.Functions.Like(x.MaterialCode, $"%{term}%")) ||
            (x.MaterialDescription != null && EF.Functions.Like(x.MaterialDescription, $"%{term}%")));
    }

    var totalCount = await query.CountAsync();

    // ── Sort ─────────────────────────────────────────────────────────
    bool asc = sortDir?.ToLower() != "desc";
    query = sortColumn?.ToLower() switch
    {
        "startdate" => asc ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
        "enddate"   => asc ? query.OrderBy(x => x.EndDate)   : query.OrderByDescending(x => x.EndDate),
        "rate"      => asc ? query.OrderBy(x => x.Rate)      : query.OrderByDescending(x => x.Rate),
        _           => asc ? query.OrderBy(x => x.MaterialCode).ThenBy(x => x.StartDate)
                           : query.OrderByDescending(x => x.MaterialCode).ThenBy(x => x.StartDate),
    };

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, totalCount);
}
        public async Task<IEnumerable<Material>> GetMaterialsAsync()
            => await _context.Materials
                .Where(m => m.Deleted != true)
                .OrderBy(m => m.Code)
                .ToListAsync();

        public async Task<MaterialRateVM> CreateAsync(MaterialRateVM model)
        {
            var entity = new MaterialRate
            {
                EnteredDate = DateTime.UtcNow,
                MaterialObjectId = model.MaterialObjectId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Rate = model.Rate,
                Deleted = false
            };

            _context.MaterialsRate.Add(entity);
            await _context.SaveChangesAsync();

            model.ObjectID = entity.ObjectID;
            return model;
        }

        public async Task<(bool Success, string Message)> BulkUpdateEndDateAsync(
            IEnumerable<int> objectIds,
            DateTime newEndDate)
        {
            var ids = objectIds.ToList();
            var rows = await _context.MaterialsRate
                .Where(r => ids.Contains(r.ObjectID) && r.Deleted != true)
                .ToListAsync();

            if (!rows.Any())
                return (false, "No matching records found.");

            foreach (var row in rows)
                row.EndDate = newEndDate;

            await _context.SaveChangesAsync();
            return (true, $"{rows.Count} record(s) updated.");
        }

        public async Task<(bool Success, string Message)> UpdateRateAsync(
            int objectId,
            decimal rate,
            DateTime? startDate,
            DateTime? endDate)
        {
            var entity = await _context.MaterialsRate.FindAsync(objectId);
            if (entity == null || entity.Deleted == true)
                return (false, "Record not found.");

            entity.Rate = rate;
            entity.StartDate = startDate;
            entity.EndDate = endDate;

            await _context.SaveChangesAsync();
            return (true, "Rate updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(IEnumerable<int> objectIds)
        {
            var ids = objectIds.ToList();
            var rows = await _context.MaterialsRate
                .Where(r => ids.Contains(r.ObjectID) && r.Deleted != true)
                .ToListAsync();

            if (!rows.Any())
                return (false, "No matching records found.");

            foreach (var row in rows)
                row.Deleted = true;   // soft delete

            await _context.SaveChangesAsync();
            return (true, $"{rows.Count} record(s) deleted.");
        }
    }
}
