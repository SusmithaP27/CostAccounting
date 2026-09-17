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
            bool showAllDates,
            DateTime? startDate,
            DateTime? endDate,
            string? searchTerm,
            string? sortColumn,
            string? sortDir,
            int page,
            int pageSize)
        {
            var query = _context.MaterialsRate
                .Where(r => r.Deleted != true)
                .Join(_context.Materials,
                      r => r.MaterialObjectId,
                      m => m.ObjectID,
                      (r, m) => new MaterialRateVM
                      {
                          MaterialObjectId = r.MaterialObjectId,
                          MaterialCode = m.Code,
                          MaterialDescription = m.Description,
                          StartDate = r.StartDate,
                          EndDate = r.EndDate,
                          Rate = r.Rate
                      });

            // ── Material filter ──────────────────────────────────────────────
            // Only filter by material when showAllMaterials is false AND a material is selected
            if (!showAllMaterials && filterMaterialId.HasValue && filterMaterialId.Value > 0)
                query = query.Where(r => r.MaterialObjectId == filterMaterialId.Value);

            // ── Date filter ──────────────────────────────────────────────────
            // Only apply when showAllDates is false AND dates are provided
            if (!showAllDates && startDate.HasValue && endDate.HasValue)
            {
                var sd = startDate.Value.Date;
                var ed = endDate.Value.Date;
                // Show rates that are active within the selected date range
                // i.e. StartDate <= filterEnd AND (EndDate >= filterStart OR EndDate is null)
                query = query.Where(r =>
                    (r.StartDate == null || r.StartDate.Value <= ed) &&
                    (r.EndDate == null || r.EndDate.Value >= sd));
            }

            // ── Search — use EF-safe Contains (case-insensitive on SQL Server) ──
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(r =>
                    (r.MaterialCode != null && EF.Functions.Like(r.MaterialCode, $"%{term}%")) ||
                    (r.MaterialDescription != null && EF.Functions.Like(r.MaterialDescription, $"%{term}%")));
            }

            // ── Total count before paging ────────────────────────────────────
            var totalCount = await query.CountAsync();

            // ── Sort ─────────────────────────────────────────────────────────
            bool asc = sortDir?.ToLower() != "desc";
            query = sortColumn?.ToLower() switch
            {
                "materialdescription" => asc ? query.OrderBy(x => x.MaterialDescription) : query.OrderByDescending(x => x.MaterialDescription),
                "startdate" => asc ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
                "enddate" => asc ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate),
                "rate" => asc ? query.OrderBy(x => x.Rate) : query.OrderByDescending(x => x.Rate),

                _ => asc ? query.OrderBy(x => x.MaterialCode) : query.OrderByDescending(x => x.MaterialCode),
            };

            // ── Page ─────────────────────────────────────────────────────────
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
                .Where(r => ids.Contains(r.ObjectID))
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
    }
}
