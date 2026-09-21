using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            if (!filter.ShowInactiveEmployees)
                query = query.Where(er => er.Employee.Active);

            if (filter.EmployeeObjectId.HasValue)
                query = query.Where(er => er.EmployeeObjectId == filter.EmployeeObjectId);

            if (!filter.ShowAll)
                query = query.Where(er => er.EndDate == null || er.EndDate >= DateTime.Today);

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

            query = filter.SortField switch
            {
                "EmployeeName" => filter.SortDirection == "asc"
                    ? query.OrderBy(er => er.Employee.LastName)
                    : query.OrderByDescending(er => er.Employee.LastName),
                "Rate" => filter.SortDirection == "asc" ? query.OrderBy(er => er.Rate) : query.OrderByDescending(er => er.Rate),
                "EndDate" => filter.SortDirection == "asc" ? query.OrderBy(er => er.EndDate) : query.OrderByDescending(er => er.EndDate),
                _ => filter.SortDirection == "asc" ? query.OrderBy(er => er.StartDate) : query.OrderByDescending(er => er.StartDate),
            };

            var page = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(er => new EmployeeRateVM
                {
                    ObjectID = er.ObjectID,
                    EmployeeObjectId = er.EmployeeObjectId,
                    EmployeeName = er.Employee.LastName + ", " + er.Employee.FirstName,
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
    }
}
