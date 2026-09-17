using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.OperationService
{
    public class OperationService : IOperationService
    {
        private readonly ApplicationDbContext _context;

        public OperationService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<OperationVM> GetPagedOperationsAsync(
            string? searchTerm,
            bool? filterActive,
            bool? filterEngineering,
            int page,
            int pageSize,
            string? sortColumn = "Code",
            string? sortDirection = "asc")
        {
            var query = _context.Operations
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
                "description"   => desc ? query.OrderByDescending(r => r.Description)   : query.OrderBy(r => r.Description),
                "enteredbyuser" => desc ? query.OrderByDescending(r => r.EnteredByUser) : query.OrderBy(r => r.EnteredByUser),
                "entereddate"   => desc ? query.OrderByDescending(r => r.EnteredDate)   : query.OrderBy(r => r.EnteredDate),
                "active"        => desc ? query.OrderByDescending(r => r.Active)        : query.OrderBy(r => r.Active),
                "engineering"   => desc ? query.OrderByDescending(r => r.Engineering)   : query.OrderBy(r => r.Engineering),
                _               => desc ? query.OrderByDescending(r => r.Code)          : query.OrderBy(r => r.Code),
            };

            var operations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new OperationVM
            {
                Operations = operations,
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

        public async Task<Operation?> GetByIdAsync(short objectId)
        {
            return await _context.Operations.FindAsync(objectId);
        }

        public async Task<(bool Success, string Message)> CreateAsync(Operation operation)
        {
            try
            {
                operation.EnteredDate = DateTime.Now;
                operation.Active = true;
                operation.Deleted = false;
                operation.Engineering = false;

                _context.Operations.Add(operation);
                await _context.SaveChangesAsync();
                return (true, "Operation created successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateInlineAsync(short objectId, bool? active, bool? engineering)
        {
            var operation = await _context.Operations.FindAsync(objectId);
            if (operation == null)
                return (false, "Record not found.");

            try
            {
                operation.Active = active;
                operation.Engineering = engineering;
                await _context.SaveChangesAsync();
                return (true, "Record updated.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var operation = await _context.Operations.FindAsync(objectId);
            if (operation == null)
                return (false, "Record not found.");

            try
            {
                operation.Deleted = true;
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