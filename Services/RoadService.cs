// using CostAccounting.Data;
// using CostAccounting.Models;
// using Microsoft.EntityFrameworkCore;
// namespace CostAccounting.Services
// {
//     public class RoadService : IRoadService
//     {
//         private readonly ApplicationDbContext _context;

//         public RoadService(ApplicationDbContext context)
//         {
//             _context = context;
//         }
//         public async Task<RoadVM> GetPagedRoadsAsync(
//             string? searchTerm,
//             bool? filterActive,
//             bool? filterEngineering,
//             int page,
//             int pageSize)
//         {
//             var query = _context.Roads
//                 .Where(r => r.Deleted != true)
//                 .AsQueryable();

//             if (!string.IsNullOrWhiteSpace(searchTerm))
//             {
//                 query = query.Where(r =>
//                     (r.Code != null && r.Code.Contains(searchTerm)) ||
//                     (r.Description != null && r.Description.Contains(searchTerm)) ||
//                     (r.EnteredByUser != null && r.EnteredByUser.Contains(searchTerm)));
//             }

//             if (filterActive.HasValue)
//                 query = query.Where(r => r.Active == filterActive.Value);

//             if (filterEngineering.HasValue)
//                 query = query.Where(r => r.Engineering == filterEngineering.Value);

//             var totalCount = await query.CountAsync();
//             var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

//             var roads = await query
//                 .OrderBy(r => r.Code)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();

//             return new RoadVM
//             {
//                 Roads = roads,
//                 SearchTerm = searchTerm,
//                 FilterActive = filterActive,
//                 FilterEngineering = filterEngineering,
//                 CurrentPage = page,
//                 TotalPages = totalPages,
//                 PageSize = pageSize,
//                 TotalCount = totalCount
//             };
//         }

//         public async Task<Road?> GetByIdAsync(short objectId)
//         {
//             return await _context.Roads.FindAsync(objectId);
//         }

//         public async Task<(bool Success, string Message)> CreateAsync(Road road)
//         {
//             try
//             {
//                 road.EnteredDate = DateTime.Now;
//                 road.Active = true;
//                 road.Deleted = false;
//                 road.Engineering = false;

//                 _context.Roads.Add(road);
//                 await _context.SaveChangesAsync();
//                 return (true, "Road created successfully.");
//             }
//             catch (Exception ex)
//             {
//                 return (false, ex.Message);
//             }
//         }

//         public async Task<(bool Success, string Message)> UpdateInlineAsync(short objectId, bool? active, bool? engineering)
//         {
//             var road = await _context.Roads.FindAsync(objectId);
//             if (road == null)
//                 return (false, "Record not found.");

//             try
//             {
//                 road.Active = active;
//                 road.Engineering = engineering;
//                 await _context.SaveChangesAsync();
//                 return (true, "Record updated.");
//             }
//             catch (Exception ex)
//             {
//                 return (false, ex.Message);
//             }
//         }

//         public async Task<(bool Success, string Message)> SoftDeleteAsync(short objectId)
//         {
//             var road = await _context.Roads.FindAsync(objectId);
//             if (road == null)
//                 return (false, "Record not found.");

//             try
//             {
//                 road.Deleted = true;
//                 await _context.SaveChangesAsync();
//                 return (true, "Record deleted.");
//             }
//             catch (Exception ex)
//             {
//                 return (false, ex.Message);
//             }
//         }

//     }
// }
using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;
namespace CostAccounting.Services
{
    public class RoadService : IRoadService
    {
        private readonly ApplicationDbContext _context;

        public RoadService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<RoadVM> GetPagedRoadsAsync(
            string? searchTerm,
            bool? filterActive,
            bool? filterEngineering,
            int page,
            int pageSize,
            string? sortColumn = "Code",
            string? sortDirection = "asc")
        {
            var query = _context.Roads
                .Where(r => r.Deleted != true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    (r.Code != null && r.Code.Contains(searchTerm)) ||
                    (r.Description != null && r.Description.Contains(searchTerm)) ||
                    (r.EnteredByUser != null && r.EnteredByUser.Contains(searchTerm)));
            }

            if (filterActive.HasValue)
                query = query.Where(r => r.Active == filterActive.Value);

            if (filterEngineering.HasValue)
                query = query.Where(r => r.Engineering == filterEngineering.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = (sortColumn?.ToLower()) switch
            {
                "description"    => desc ? query.OrderByDescending(r => r.Description)    : query.OrderBy(r => r.Description),
                "enteredbyuser"  => desc ? query.OrderByDescending(r => r.EnteredByUser)  : query.OrderBy(r => r.EnteredByUser),
                "entereddate"    => desc ? query.OrderByDescending(r => r.EnteredDate)    : query.OrderBy(r => r.EnteredDate),
                "active"         => desc ? query.OrderByDescending(r => r.Active)         : query.OrderBy(r => r.Active),
                _                => desc ? query.OrderByDescending(r => r.Code)           : query.OrderBy(r => r.Code),
            };

            var roads = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new RoadVM
            {
                Roads = roads,
                SearchTerm = searchTerm,
                FilterActive = filterActive,
                FilterEngineering = filterEngineering,
                SortColumn = sortColumn ?? "Code",
                SortDirection = sortDirection ?? "asc",
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Road?> GetByIdAsync(short objectId)
        {
            return await _context.Roads.FindAsync(objectId);
        }

        public async Task<(bool Success, string Message)> CreateAsync(Road road)
        {
            try
            {
                road.EnteredDate = DateTime.Now;
                road.Active = true;
                road.Deleted = false;
                road.Engineering = false;

                _context.Roads.Add(road);
                await _context.SaveChangesAsync();
                return (true, "Road created successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateInlineAsync(short objectId, bool? active, bool? engineering)
        {
            var road = await _context.Roads.FindAsync(objectId);
            if (road == null)
                return (false, "Record not found.");

            try
            {
                road.Active = active;
                road.Engineering = engineering;
                await _context.SaveChangesAsync();
                return (true, "Record updated.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> SoftDeleteAsync(short objectId)
        {
            var road = await _context.Roads.FindAsync(objectId);
            if (road == null)
                return (false, "Record not found.");

            try
            {
                road.Deleted = true;
                await _context.SaveChangesAsync();
                return (true, "Record deleted.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}