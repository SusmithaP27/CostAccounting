// using CostAccounting.Data;
// using CostAccounting.Models;
// using CostAccounting.Models.ViewModels;
// using Microsoft.EntityFrameworkCore;

// namespace CostAccounting.Services
// {
//     public class AllowanceService : IAllowanceService
//     {
//         private readonly ApplicationDbContext _context;

//         public AllowanceService(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<AllowanceIndexVM> GetAllowancesAsync(
//             int? filterYear, bool? filterActive, int? filterSubTypeId,
//             string sortColumn, string sortDirection, int page, int pageSize)
//         {
//             var query =
//                 from a in _context.Allowances
//                 join lk in _context.Lookups on a.AllowanceSubTypeObjectID equals lk.ObjectID into lkJoin
//                 from lk in lkJoin.DefaultIfEmpty()
//                 where a.Deleted != true
//                 select new AllowanceVM
//                 {
//                     ObjectID = a.ObjectID,
//                     AllowanceTypeObjectID = a.AllowanceTypeObjectID,
//                     Year = a.Year,
//                     Rate = a.Rate,
//                     AllowanceSubTypeObjectID = a.AllowanceSubTypeObjectID,
//                     SubTypeName = lk != null ? lk.Code : null,
//                     EnteredDate = a.EnteredDate,
//                     EnteredByUser = a.EnteredByUser,
//                     Active = a.Active
//                 };

//             // if (filterYear.HasValue)
//             //     query = query.Where(a => a.Year == filterYear);
//             if (filterYear.HasValue){
//                 short? yearFilter = (short)filterYear.Value;
//                 query = query.Where(a => a.Year == yearFilter);
//             }

//             if (filterActive.HasValue)
//                 query = query.Where(a => a.Active == filterActive);

//             if (filterSubTypeId.HasValue)
//                 query = query.Where(a => a.AllowanceSubTypeObjectID == filterSubTypeId);

//             bool asc = sortDirection?.ToLower() == "asc";
//             query = (sortColumn, asc) switch
//             {
//                 ("Year", true) => query.OrderBy(a => a.Year),
//                 ("Year", false) => query.OrderByDescending(a => a.Year),
//                 ("Rate", true) => query.OrderBy(a => a.Rate),
//                 ("Rate", false) => query.OrderByDescending(a => a.Rate),
//                 ("Type", true) => query.OrderBy(a => a.SubTypeName),
//                 ("Type", false) => query.OrderByDescending(a => a.SubTypeName),
//                 ("EnteredDate", true) => query.OrderBy(a => a.EnteredDate),
//                 ("EnteredDate", false) => query.OrderByDescending(a => a.EnteredDate),
//                 _ => query.OrderByDescending(a => a.Year)
//             };

//             // var totalCount = query.Count();
//             var totalCount = await query.CountAsync();

//             var results = query
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToList();

//             return new AllowanceIndexVM
//             {
//                 Allowances = results,
//                 SubTypes = await GetSubTypesAsync(),
//                 FilterYear = filterYear,
//                 FilterActive = filterActive,
//                 FilterSubTypeId = filterSubTypeId,
//                 SortColumn = sortColumn,
//                 SortDirection = sortDirection,
//                 CurrentPage = page,
//                 PageSize = pageSize,
//                 TotalCount = totalCount
//             };
//         }

//         public async Task<List<Lookup>> GetSubTypesAsync()
//         {
//             return await _context.Lookups
//                 .Where(lk => lk.LookupCategoryObjectID == 8
//                              && lk.Deleted != true
//                              && lk.Active == true
//                              && !string.IsNullOrEmpty(lk.Code))
//                 .OrderBy(lk => lk.DisplayOrder)
//                 .ToListAsync();
//         }

//         public async Task<(bool Success, string Message)> CreateAllowanceAsync(
//             short? year, decimal? rate, int? subTypeId, bool active, string enteredByUser)
//         {
//             if (year == null) return (false, "Year is required.");
//             if (rate == null) return (false, "Rate is required.");
//             if (subTypeId == null) return (false, "Type is required.");

//             var allowance = new Allowance
//             {
//                 AllowanceTypeObjectID = 23,
//                 Year = year,
//                 Rate = rate,
//                 AllowanceSubTypeObjectID = subTypeId,
//                 EnteredDate = DateTime.Now,
//                 EnteredByUser = enteredByUser,
//                 Active = active,
//                 Deleted = false
//             };

//             _context.Allowances.Add(allowance);
//             await _context.SaveChangesAsync();

//             return (true, "Allowance added successfully.");
//         }

//         public async Task<(bool Success, string Message)> UpdateAllowanceAsync(
//             int objectId, short? year, decimal? rate, int? subTypeId, bool active)
//         {
//             var allowance = await _context.Allowances.FindAsync(objectId);
//             if (allowance == null) return (false, "Allowance not found.");

//             if (year == null) return (false, "Year is required.");
//             if (rate == null) return (false, "Rate is required.");
//             if (subTypeId == null) return (false, "Type is required.");

//             allowance.Year = year;
//             allowance.Rate = rate;
//             allowance.AllowanceSubTypeObjectID = subTypeId;
//             allowance.Active = active;

//             await _context.SaveChangesAsync();
//             return (true, "Allowance updated.");
//         }

//         public async Task<(bool Success, string Message)> DeleteAllowancesAsync(List<int> objectIds)
//         {
//             if (objectIds == null || objectIds.Count == 0)
//                 return (false, "No rows selected.");

//             var rows = new List<Allowance>();
//             foreach (var id in objectIds)
//             {
//                 var row = await _context.Allowances.FindAsync(id);
//                 if (row != null) rows.Add(row);
//             }

//             foreach (var row in rows)
//                 row.Deleted = true;

//             await _context.SaveChangesAsync();
//             return (true, $"{rows.Count} row(s) deleted.");
//         }
//     }
// }
using CostAccounting.Data;
using CostAccounting.Models;
using CostAccounting.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services
{
    public class AllowanceService : IAllowanceService
    {
        private readonly ApplicationDbContext _context;

        public AllowanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AllowanceIndexVM> GetAllowancesAsync(
            int? filterYear, bool? filterActive, int? filterSubTypeId,
            string sortColumn, string sortDirection, int page, int pageSize)
        {
            var query =
                from a in _context.Allowances
                join lk in _context.Lookups on a.AllowanceSubTypeObjectID equals lk.ObjectID into lkJoin
                from lk in lkJoin.DefaultIfEmpty()
                where a.Deleted != true
                select new AllowanceVM
                {
                    ObjectID = a.ObjectID,
                    AllowanceTypeObjectID = a.AllowanceTypeObjectID,
                    Year = a.Year,
                    Rate = a.Rate,
                    AllowanceSubTypeObjectID = a.AllowanceSubTypeObjectID,
                    SubTypeName = lk != null ? lk.Code : null,
                    EnteredDate = a.EnteredDate,
                    EnteredByUser = a.EnteredByUser,
                    Active = a.Active
                };

            if (filterYear.HasValue)
            {
                short yearFilter = (short)filterYear.Value;
                query = query.Where(a => a.Year == yearFilter);
            }

            if (filterActive.HasValue)
                query = query.Where(a => a.Active == filterActive);

            if (filterSubTypeId.HasValue)
                query = query.Where(a => a.AllowanceSubTypeObjectID == filterSubTypeId);

            bool asc = sortDirection?.ToLower() == "asc";
            query = (sortColumn, asc) switch
            {
                ("Year", true) => query.OrderBy(a => a.Year),
                ("Year", false) => query.OrderByDescending(a => a.Year),
                ("Rate", true) => query.OrderBy(a => a.Rate),
                ("Rate", false) => query.OrderByDescending(a => a.Rate),
                ("Type", true) => query.OrderBy(a => a.SubTypeName),
                ("Type", false) => query.OrderByDescending(a => a.SubTypeName),
                ("EnteredDate", true) => query.OrderBy(a => a.EnteredDate),
                ("EnteredDate", false) => query.OrderByDescending(a => a.EnteredDate),
                _ => query.OrderByDescending(a => a.Year)
            };

            var totalCount = await query.CountAsync();

            var results = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AllowanceIndexVM
            {
                Allowances = results,
                SubTypes = await GetSubTypesAsync(),
                FilterYear = filterYear,
                FilterActive = filterActive,
                FilterSubTypeId = filterSubTypeId,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<Lookup>> GetSubTypesAsync()
        {
            return await _context.Lookups
                .Where(lk => lk.LookupCategoryObjectID == 8
                             && lk.Deleted != true
                             && lk.Active == true
                             && !string.IsNullOrEmpty(lk.Code))
                .OrderBy(lk => lk.DisplayOrder)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateAllowanceAsync(
            short? year, decimal? rate, int? subTypeId, bool active, string enteredByUser)
        {
            if (year == null) return (false, "Year is required.");
            if (rate == null) return (false, "Rate is required.");
            if (subTypeId == null) return (false, "Type is required.");

            var allowance = new Allowance
            {
                AllowanceTypeObjectID = 23,
                Year = year,
                Rate = rate,
                AllowanceSubTypeObjectID = subTypeId,
                EnteredDate = DateTime.Now,
                EnteredByUser = enteredByUser,
                Active = active,
                Deleted = false
            };

            _context.Allowances.Add(allowance);
            await _context.SaveChangesAsync();

            return (true, "Allowance added successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAllowanceAsync(
            int objectId, short? year, decimal? rate, int? subTypeId, bool active)
        {
            var allowance = await _context.Allowances.FindAsync(objectId);
            if (allowance == null) return (false, "Allowance not found.");

            if (year == null) return (false, "Year is required.");
            if (rate == null) return (false, "Rate is required.");
            if (subTypeId == null) return (false, "Type is required.");

            allowance.Year = year;
            allowance.Rate = rate;
            allowance.AllowanceSubTypeObjectID = subTypeId;
            allowance.Active = active;

            await _context.SaveChangesAsync();
            return (true, "Allowance updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAllowancesAsync(List<int> objectIds)
        {
            if (objectIds == null || objectIds.Count == 0)
                return (false, "No rows selected.");

            var rows = new List<Allowance>();
            foreach (var id in objectIds)
            {
                var row = await _context.Allowances.FindAsync(id);
                if (row != null) rows.Add(row);
            }

            foreach (var row in rows)
                row.Deleted = true;

            await _context.SaveChangesAsync();
            return (true, $"{rows.Count} row(s) deleted.");
        }
    }
}