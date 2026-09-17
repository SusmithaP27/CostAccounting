// using CostAccounting.Data;
// using CostAccounting.Models;
// using Microsoft.EntityFrameworkCore;

// namespace CostAccounting.Services.EquipmentRateService
// {
//     public class EquipmentRateService : IEquipmentRateService
//     {
//         private readonly ApplicationDbContext _context;

//         public EquipmentRateService(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<IEnumerable<EquipmentLookupVM>> GetEquipmentListAsync()
//         {
//             return await _context.Equipments
//                 .Where(e => e.Deleted != true)
//                 .OrderBy(e => e.Code)
//                 .Select(e => new EquipmentLookupVM
//                 {
//                     ObjectID = e.ObjectID,
//                     Code = e.Code,
//                     Make = e.Make,
//                     Model = e.Model
//                 })
//                 .ToListAsync();
//         }

//         public async Task<(IEnumerable<EquipmentRateVM> Items, int TotalCount)> GetRatesAsync(
//             int? filterEquipmentId,
//             bool showAllEquipment,
//             string? startDateOperator,
//             DateTime? startDateFilter,
//             string? endDateOperator,
//             DateTime? endDateFilter,
//             bool showAllDates,
//             string? searchTerm,
//             int page,
//             int pageSize)
//         {
//             var query =
//                 from r in _context.EquipmentsRate
//                 where r.Deleted != true

//                 join e in _context.Equipments
//                     on r.EquipmentObjectId equals e.ObjectID

//                 select new EquipmentRateVM
//                 {
//                     ObjectID = r.ObjectID,
//                     EquipmentObjectId = r.EquipmentObjectId,
//                     EquipmentCode = e.Code,
//                     EquipmentMake = e.Make,
//                     EquipmentModel = e.Model,
//                     StartDate = r.StartDate,
//                     EndDate = r.EndDate,
//                     Rate = r.Rate,
//                     MiscRate = r.MiscRate
//                 };

//             // ── Equipment filter ───────────────────────────────────────────
//             if (!showAllEquipment && filterEquipmentId.HasValue)
//                 query = query.Where(x => x.EquipmentObjectId == filterEquipmentId.Value);

//             // ── Date range filter ──────────────────────────────────────────
//             if (!showAllDates)
//             {
//                 if (startDateFilter.HasValue)
//                 {
//                     query = (startDateOperator ?? "<=") switch
//                     {
//                         "<" => query.Where(x => x.StartDate < startDateFilter.Value),
//                         "<=" => query.Where(x => x.StartDate <= startDateFilter.Value),
//                         ">" => query.Where(x => x.StartDate > startDateFilter.Value),
//                         ">=" => query.Where(x => x.StartDate >= startDateFilter.Value),
//                         "=" => query.Where(x => x.StartDate == startDateFilter.Value),
//                         _ => query.Where(x => x.StartDate <= startDateFilter.Value),
//                     };
//                 }

//                 if (endDateFilter.HasValue)
//                 {
//                     query = (endDateOperator ?? ">=") switch
//                     {
//                         "<" => query.Where(x => x.EndDate < endDateFilter.Value),
//                         "<=" => query.Where(x => x.EndDate <= endDateFilter.Value),
//                         ">" => query.Where(x => x.EndDate > endDateFilter.Value),
//                         ">=" => query.Where(x => x.EndDate >= endDateFilter.Value),
//                         "=" => query.Where(x => x.EndDate == endDateFilter.Value),
//                         _ => query.Where(x => x.EndDate >= endDateFilter.Value),
//                     };
//                 }
//             }

//             // ── Search ─────────────────────────────────────────────────────
//             if (!string.IsNullOrWhiteSpace(searchTerm))
//             {
//                 var term = searchTerm.Trim().ToLower();
//                 query = query.Where(x =>
//                     (x.EquipmentCode != null && x.EquipmentCode.ToLower().Contains(term)) ||
//                     (x.EquipmentMake != null && x.EquipmentMake.ToLower().Contains(term)) ||
//                     (x.EquipmentModel != null && x.EquipmentModel.ToLower().Contains(term)));
//             }

//             // ── Total count before paging ──────────────────────────────────
//             var totalCount = await query.CountAsync();

//             // ── Sort + Page ────────────────────────────────────────────────
//             var items = await query
//                 .OrderBy(x => x.EquipmentCode)
//                 .ThenBy(x => x.StartDate)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();

//             return (items, totalCount);
//         }

//         public async Task<EquipmentRateVM> CreateAsync(EquipmentRateVM model)
//         {
//             var entity = new EquipmentRate
//             {
//                 EquipmentObjectId = model.EquipmentObjectId,
//                 StartDate = model.StartDate,
//                 EndDate = model.EndDate,
//                 Rate = model.Rate,
//                 MiscRate = model.MiscRate,
//                 EnteredDate = DateTime.UtcNow,
//                 Deleted = false
//             };

//             _context.EquipmentsRate.Add(entity);
//             await _context.SaveChangesAsync();

//             model.ObjectID = entity.ObjectID;
//             return model;
//         }

//         public async Task<EquipmentRateVM?> UpdateAsync(EquipmentRateVM model)
//         {
//             var entity = await _context.EquipmentsRate.FindAsync(model.ObjectID);
//             if (entity == null || entity.Deleted == true) return null;

//             entity.EquipmentObjectId = model.EquipmentObjectId;
//             entity.StartDate = model.StartDate;
//             entity.EndDate = model.EndDate;
//             entity.Rate = model.Rate;
//             entity.MiscRate = model.MiscRate;

//             await _context.SaveChangesAsync();
//             return model;
//         }

//         public async Task<int> BulkUpdateEndDateAsync(List<int> objectIds, DateTime newEndDate)
//         {
//             if (objectIds == null || objectIds.Count == 0) return 0;

//             var updatedCount = 0;
//             foreach (var id in objectIds)
//             {
//                 var entity = await _context.EquipmentsRate.FindAsync(id);
//                 if (entity == null || entity.Deleted == true) continue;

//                 entity.EndDate = newEndDate;
//                 updatedCount++;
//             }

//             await _context.SaveChangesAsync();
//             return updatedCount;
//         }

//         public async Task<int> BulkDeleteAsync(List<int> objectIds)
//         {
//             if (objectIds == null || objectIds.Count == 0) return 0;

//             var deletedCount = 0;
//             foreach (var id in objectIds)
//             {
//                 var entity = await _context.EquipmentsRate.FindAsync(id);
//                 if (entity == null || entity.Deleted == true) continue;

//                 entity.Deleted = true;
//                 deletedCount++;
//             }

//             await _context.SaveChangesAsync();
//             return deletedCount;
//         }
//     }
// }
using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.EquipmentRateService
{
    public class EquipmentRateService : IEquipmentRateService
    {
        private readonly ApplicationDbContext _context;

        public EquipmentRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EquipmentLookupVM>> GetEquipmentListAsync()
        {
            return await _context.Equipments
                .Where(e => e.Deleted != true)
                .OrderBy(e => e.Code)
                .Select(e => new EquipmentLookupVM
                {
                    ObjectID = e.ObjectID,
                    Code = e.Code,
                    Make = e.Make,
                    Model = e.Model
                })
                .ToListAsync();
        }

        public async Task<(IEnumerable<EquipmentRateVM> Items, int TotalCount)> GetRatesAsync(
            int? filterEquipmentId,
            bool showAllEquipment,
            string? startDateOperator,
            DateTime? startDateFilter,
            string? endDateOperator,
            DateTime? endDateFilter,
            bool showAllDates,
            string? searchTerm,
            int page,
            int pageSize)
        {
            var query =
                from r in _context.EquipmentsRate
                where r.Deleted != true

                join e in _context.Equipments
                    on r.EquipmentObjectId equals e.ObjectID

                select new EquipmentRateVM
                {
                    ObjectID = r.ObjectID,
                    EquipmentObjectId = r.EquipmentObjectId,
                    EquipmentCode = e.Code,
                    EquipmentMake = e.Make,
                    EquipmentModel = e.Model,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Rate = r.Rate,
                    MiscRate = r.MiscRate
                };

            // ── Equipment filter ───────────────────────────────────────────
            if (!showAllEquipment && filterEquipmentId.HasValue)
                query = query.Where(x => x.EquipmentObjectId == filterEquipmentId.Value);

            // ── Date range filter ──────────────────────────────────────────
            // Each side (Start / End) is applied independently — leaving one
            // date box empty on the filter bar leaves that side unbounded,
            // as long as showAllDates is false and the other side still has
            // a value (or is also left open).
            if (!showAllDates)
            {
                if (startDateFilter.HasValue)
                {
                    query = (startDateOperator ?? "<=") switch
                    {
                        "<" => query.Where(x => x.StartDate < startDateFilter.Value),
                        "<=" => query.Where(x => x.StartDate <= startDateFilter.Value),
                        ">" => query.Where(x => x.StartDate > startDateFilter.Value),
                        ">=" => query.Where(x => x.StartDate >= startDateFilter.Value),
                        "=" => query.Where(x => x.StartDate == startDateFilter.Value),
                        _ => query.Where(x => x.StartDate <= startDateFilter.Value),
                    };
                }

                if (endDateFilter.HasValue)
                {
                    query = (endDateOperator ?? ">=") switch
                    {
                        "<" => query.Where(x => x.EndDate < endDateFilter.Value),
                        "<=" => query.Where(x => x.EndDate <= endDateFilter.Value),
                        ">" => query.Where(x => x.EndDate > endDateFilter.Value),
                        ">=" => query.Where(x => x.EndDate >= endDateFilter.Value),
                        "=" => query.Where(x => x.EndDate == endDateFilter.Value),
                        _ => query.Where(x => x.EndDate >= endDateFilter.Value),
                    };
                }
            }

            // ── Search ─────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.EquipmentCode != null && x.EquipmentCode.ToLower().Contains(term)) ||
                    (x.EquipmentMake != null && x.EquipmentMake.ToLower().Contains(term)) ||
                    (x.EquipmentModel != null && x.EquipmentModel.ToLower().Contains(term)));
            }

            // ── Total count before paging ──────────────────────────────────
            var totalCount = await query.CountAsync();

            // ── Sort + Page ────────────────────────────────────────────────
            var items = await query
                .OrderBy(x => x.EquipmentCode)
                .ThenBy(x => x.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<EquipmentRateVM> CreateAsync(EquipmentRateVM model)
        {
            var entity = new EquipmentRate
            {
                EquipmentObjectId = model.EquipmentObjectId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Rate = model.Rate,
                MiscRate = model.MiscRate,
                EnteredDate = DateTime.UtcNow,
                Deleted = false
            };

            _context.EquipmentsRate.Add(entity);
            await _context.SaveChangesAsync();

            model.ObjectID = entity.ObjectID;
            return model;
        }

        public async Task<EquipmentRateVM?> UpdateAsync(EquipmentRateVM model)
        {
            var entity = await _context.EquipmentsRate.FindAsync(model.ObjectID);
            if (entity == null || entity.Deleted == true) return null;

            entity.EquipmentObjectId = model.EquipmentObjectId;
            entity.StartDate = model.StartDate;
            entity.EndDate = model.EndDate;
            entity.Rate = model.Rate;
            entity.MiscRate = model.MiscRate;

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<int> BulkUpdateEndDateAsync(List<int> objectIds, DateTime newEndDate)
        {
            if (objectIds == null || objectIds.Count == 0) return 0;

            var updatedCount = 0;
            foreach (var id in objectIds)
            {
                var entity = await _context.EquipmentsRate.FindAsync(id);
                if (entity == null || entity.Deleted == true) continue;

                entity.EndDate = newEndDate;
                updatedCount++;
            }

            await _context.SaveChangesAsync();
            return updatedCount;
        }

        public async Task<int> BulkDeleteAsync(List<int> objectIds)
        {
            if (objectIds == null || objectIds.Count == 0) return 0;

            var deletedCount = 0;
            foreach (var id in objectIds)
            {
                var entity = await _context.EquipmentsRate.FindAsync(id);
                if (entity == null || entity.Deleted == true) continue;

                entity.Deleted = true;
                deletedCount++;
            }

            await _context.SaveChangesAsync();
            return deletedCount;
        }
    }
}
