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

        // Code = MunisCode padded to a minimum of 4 digits with leading zeros.
        // 234 -> "0234", 1234 -> "1234", 12234 -> "12234".
        public static string GenerateCodeFromMunisCode(int munisCode)
        {
            return munisCode.ToString().PadLeft(4, '0');
        }

        public async Task<List<EmployeeOptionVM>> GetActiveEmployeeOptionsAsync()
        {
            return await _context.Employees
                .Where(e => e.Active && !e.Deleted)
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => new EmployeeOptionVM
                {
                    ObjectID = e.ObjectID,
                    DisplayName = e.LastName + ", " + e.FirstName + " (" + e.Code + ")"
                })
                .ToListAsync();
        }

        public async Task<EmployeeIndexVM> GetIndexAsync(EmployeeIndexVM filter)
        {
            var query = _context.Employees
                .Where(e => !e.Deleted)
                .AsQueryable();

            if (!filter.ShowInactive)
                query = query.Where(e => e.Active);

            if (filter.EmployeeTypeFilter.HasValue)
                query = query.Where(e => e.EmployeeTypeObjectID == filter.EmployeeTypeFilter);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                var termIsNumeric = int.TryParse(term, out var termAsInt);
                query = query.Where(e =>
                    e.FirstName.Contains(term) ||
                    e.LastName.Contains(term) ||
                    e.Code.Contains(term) ||
                    (termIsNumeric && e.MunisCode == termAsInt));
            }

            filter.TotalCount = await query.CountAsync();

            query = filter.SortField switch
            {
                "FirstName" => filter.SortDirection == "asc" ? query.OrderBy(e => e.FirstName) : query.OrderByDescending(e => e.FirstName),
                "Code" => filter.SortDirection == "asc" ? query.OrderBy(e => e.Code) : query.OrderByDescending(e => e.Code),
                "HireDate" => filter.SortDirection == "asc" ? query.OrderBy(e => e.HireDate) : query.OrderByDescending(e => e.HireDate),
                "LongevityDate" => filter.SortDirection == "asc" ? query.OrderBy(e => e.LongevityDate) : query.OrderByDescending(e => e.LongevityDate),
                _ => filter.SortDirection == "asc" ? query.OrderBy(e => e.LastName) : query.OrderByDescending(e => e.LastName),
            };

            // Materialize the raw entities first, then map to VM in memory (matches the
            // pattern used for the earlier SSN-masking fix, and keeps any string manipulation
            // off the SQL translation path).
            var entities = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Current/base rate for each employee on this page. Pulled as a separate query
            // (all currently-active rate rows, then filtered in memory to this page's
            // employee IDs) rather than a Contains()-based join — same workaround used
            // elsewhere in this app to avoid EF Core's Contains() -> CTE translation issue.
            var employeeIdsOnPage = entities.Select(e => e.ObjectID).ToHashSet();
            var currentRatesByEmployee = (await _context.EmployeeRates
                    .Where(er => !er.Deleted && (er.EndDate == null || er.EndDate >= DateTime.Today))
                    .ToListAsync())
                .Where(er => employeeIdsOnPage.Contains(er.EmployeeObjectId))
                .GroupBy(er => er.EmployeeObjectId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(er => er.StartDate).First());

            filter.Employees = entities.Select(e =>
            {
                currentRatesByEmployee.TryGetValue(e.ObjectID, out var currentRate);
                return new EmployeeVM
                {
                    ObjectID = e.ObjectID,
                    Code = e.Code,
                    MunisCode = e.MunisCode,
                    FirstName = e.FirstName,
                    MI = e.MI,
                    LastName = e.LastName,
                    Shift = e.Shift,
                    LongevityRate = e.LongevityRate,
                    LongevityDate = e.LongevityDate,
                    TitleCode = e.TitleCode,
                    TerminationDate = e.TerminationDate,
                    HireDate = e.HireDate,
                    EmployeeTypeObjectID = e.EmployeeTypeObjectID,
                    AccountObjectId = e.AccountObjectId,
                    Active = e.Active,
                    BaseRate = currentRate?.BaseRate,
                    CurrentRate = currentRate?.Rate
                };
            }).ToList();

            return filter;
        }

        // Full SSN and every editable field returned here, for the Edit modal's single-record
        // fetch — never in list results.
        public async Task<EmployeeVM> GetByIdAsync(int objectId)
        {
            var e = await _context.Employees
                .Where(x => x.ObjectID == objectId && !x.Deleted)
                .FirstOrDefaultAsync();

            if (e == null) return null;

            return new EmployeeVM
            {
                ObjectID = e.ObjectID,
                Code = e.Code,
                MunisCode = e.MunisCode,
                FirstName = e.FirstName,
                MI = e.MI,
                LastName = e.LastName,
                SSN = e.SSN,
                Shift = e.Shift,
                LongevityRate = e.LongevityRate,
                LongevityDate = e.LongevityDate,
                TitleCode = e.TitleCode,
                TerminationDate = e.TerminationDate,
                HireDate = e.HireDate,
                EmployeeTypeObjectID = e.EmployeeTypeObjectID,
                AccountObjectId = e.AccountObjectId,
                Active = e.Active
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(EmployeeVM vm, string enteredByUser)
        {
            if (!vm.MunisCode.HasValue)
                return (false, "Munis Code is required.");
            if (string.IsNullOrWhiteSpace(vm.FirstName) || string.IsNullOrWhiteSpace(vm.LastName))
                return (false, "First and last name are required.");

            var entity = new Employee
            {
                Code = GenerateCodeFromMunisCode(vm.MunisCode.Value),
                MunisCode = vm.MunisCode,
                FirstName = vm.FirstName,
                MI = vm.MI,
                LastName = vm.LastName,
                HireDate = vm.HireDate,
                EnteredByUser = enteredByUser,
                EnteredDate = DateTime.Now,
                Active = true,
                Deleted = false
            };

            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();
            return (true, $"Employee created successfully (Code {entity.Code}).");
        }

        // Edits everything except rate data (BaseRate/CurrentRate on the VM are display-only
        // and intentionally never read here — rates are only ever changed through
        // IEmployeeRateService, including the seasonal adjustment).
        public async Task<(bool Success, string Message)> UpdateAsync(EmployeeVM vm, string enteredByUser)
        {
            var entity = await _context.Employees.FindAsync(vm.ObjectID);
            if (entity == null || entity.Deleted)
                return (false, "Employee not found.");

            if (vm.MunisCode.HasValue)
            {
                entity.MunisCode = vm.MunisCode;
                entity.Code = GenerateCodeFromMunisCode(vm.MunisCode.Value); // keep Code in sync with MunisCode
            }

            entity.FirstName = vm.FirstName;
            entity.MI = vm.MI;
            entity.LastName = vm.LastName;
            if (vm.SSN.HasValue)
                entity.SSN = vm.SSN; // only overwrite if resubmitted; grid never sends full SSN back
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

        // Soft delete only — sets Deleted = 1, never removes the row.
        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var entity = await _context.Employees.FindAsync(objectId);
            if (entity == null)
                return (false, "Employee not found.");

            entity.Deleted = true;
            await _context.SaveChangesAsync();
            return (true, "Employee deleted successfully.");
        }
    }
}
