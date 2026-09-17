using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CostAccounting.Models;
using CostAccounting.Data;

namespace CostAccounting.Services.EmployeeService
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeIndexVM> GetIndexAsync(EmployeeIndexVM filter)
        {
            var query = _context.Employees
                .Where(e => !e.Deleted)
                .AsQueryable();

            if (!filter.ShowInactive)
            {
                query = query.Where(e => e.Active);
            }

            if (filter.EmployeeTypeFilter.HasValue)
            {
                query = query.Where(e =>
                    e.EmployeeTypeObjectID == filter.EmployeeTypeFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                var termIsNumeric = int.TryParse(term, out var termAsInt);

                query = query.Where(e =>
                    (e.FirstName != null && e.FirstName.Contains(term)) ||
                    (e.LastName != null && e.LastName.Contains(term)) ||
                    (e.Code != null && e.Code.Contains(term)) ||
                    (e.TitleCode != null && e.TitleCode.Contains(term)) ||
                    (termIsNumeric && e.MunisCode == termAsInt));
            }

            filter.TotalCount = await query.CountAsync();

            query = filter.SortField switch
            {
                "FirstName" =>
                    filter.SortDirection == "asc"
                        ? query.OrderBy(e => e.FirstName)
                        : query.OrderByDescending(e => e.FirstName),

                "Code" =>
                    filter.SortDirection == "asc"
                        ? query.OrderBy(e => e.Code)
                        : query.OrderByDescending(e => e.Code),

                "HireDate" =>
                    filter.SortDirection == "asc"
                        ? query.OrderBy(e => e.HireDate)
                        : query.OrderByDescending(e => e.HireDate),

                "TitleCode" =>
                    filter.SortDirection == "asc"
                        ? query.OrderBy(e => e.TitleCode)
                        : query.OrderByDescending(e => e.TitleCode),

                _ =>
                    filter.SortDirection == "asc"
                        ? query.OrderBy(e => e.LastName)
                        : query.OrderByDescending(e => e.LastName)
            };

            /*
             * IMPORTANT:
             * Do NOT materialize Employee entities first.
             *
             * Some string columns in the database contain NULL values.
             * The Employee entity currently uses non-nullable string properties.
             *
             * Project directly to EmployeeVM and convert NULL strings to "".
             * This prevents SqlNullValueException during ToListAsync().
             */
            filter.Employees = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new EmployeeVM
                {
                    ObjectID = e.ObjectID,

                    Code = e.Code ?? "",

                    MunisCode = e.MunisCode,

                    FirstName = e.FirstName ?? "",

                    MI = e.MI ?? "",

                    LastName = e.LastName ?? "",

                    Shift = e.Shift ?? "",

                    LongevityRate = e.LongevityRate,

                    LongevityDate = e.LongevityDate,

                    TitleCode = e.TitleCode ?? "",

                    TerminationDate = e.TerminationDate,

                    HireDate = e.HireDate,

                    EmployeeTypeObjectID = e.EmployeeTypeObjectID,

                    AccountObjectId = e.AccountObjectId,

                    Active = e.Active
                })
                .ToListAsync();

            return filter;
        }

        public async Task<EmployeeVM> GetByIdAsync(int objectId)
        {
            /*
             * Project directly to EmployeeVM for the same reason as GetIndexAsync:
             * database string columns may contain NULL.
             */
            var employee = await _context.Employees
                .Where(e => e.ObjectID == objectId && !e.Deleted)
                .Select(e => new EmployeeVM
                {
                    ObjectID = e.ObjectID,

                    Code = e.Code ?? "",

                    MunisCode = e.MunisCode,

                    FirstName = e.FirstName ?? "",

                    MI = e.MI ?? "",

                    LastName = e.LastName ?? "",

                    Shift = e.Shift ?? "",

                    LongevityRate = e.LongevityRate,

                    LongevityDate = e.LongevityDate,

                    TitleCode = e.TitleCode ?? "",

                    TerminationDate = e.TerminationDate,

                    HireDate = e.HireDate,

                    EmployeeTypeObjectID = e.EmployeeTypeObjectID,

                    AccountObjectId = e.AccountObjectId,

                    Active = e.Active
                })
                .FirstOrDefaultAsync();

            return employee;
        }

        public async Task<(bool Success, string Message)> CreateAsync(
            EmployeeVM vm,
            string enteredByUser)
        {
            var entity = new Employee
            {
                Code = vm.Code,
                MunisCode = vm.MunisCode,
                FirstName = vm.FirstName,
                MI = vm.MI,
                LastName = vm.LastName,
                Shift = vm.Shift,
                LongevityRate = vm.LongevityRate,
                LongevityDate = vm.LongevityDate,
                TitleCode = vm.TitleCode,
                TerminationDate = vm.TerminationDate,
                HireDate = vm.HireDate,
                EmployeeTypeObjectID = vm.EmployeeTypeObjectID,
                AccountObjectId = vm.AccountObjectId,
                EnteredByUser = enteredByUser,
                EnteredDate = DateTime.Now,
                Active = true,
                Deleted = false
            };

            _context.Employees.Add(entity);

            await _context.SaveChangesAsync();

            return (true, "Employee created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            EmployeeVM vm,
            string enteredByUser)
        {
            var entity = await _context.Employees.FindAsync(vm.ObjectID);

            if (entity == null || entity.Deleted)
            {
                return (false, "Employee not found.");
            }

            entity.Code = vm.Code;
            entity.MunisCode = vm.MunisCode;
            entity.FirstName = vm.FirstName;
            entity.MI = vm.MI;
            entity.LastName = vm.LastName;
            entity.Shift = vm.Shift;
            entity.LongevityRate = vm.LongevityRate;
            entity.LongevityDate = vm.LongevityDate;
            entity.TitleCode = vm.TitleCode;
            entity.TerminationDate = vm.TerminationDate;
            entity.HireDate = vm.HireDate;
            entity.EmployeeTypeObjectID = vm.EmployeeTypeObjectID;
            entity.AccountObjectId = vm.AccountObjectId;
            entity.Active = vm.Active;
            entity.EnteredByUser = enteredByUser;
            entity.EnteredDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, "Employee updated successfully.");
        }

        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var entity = await _context.Employees.FindAsync(objectId);

            if (entity == null)
            {
                return (false, "Employee not found.");
            }

            entity.Deleted = true;

            await _context.SaveChangesAsync();

            return (true, "Employee deleted successfully.");
        }
    }
}