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

        // Pulls LongevityDate from the GetEmployeeDate stored procedure, keyed by MunisCode.
        // TODO: confirm the real procedure name/params/columns against your DB — this assumes
        // no parameters and a result set of (MunisCode, LongevityDate) for every employee.
        // Falls back silently to the table's own LongevityDate column (per employee) if the
        // call fails, so a wrong name here doesn't take down the whole Employee grid.
        private async Task<Dictionary<int, DateTime?>> GetLongevityDatesFromStoredProcedureAsync()
        {
            try
            {
                var rows = await _context.Set<EmployeeDateInfoDto>()
                    .FromSqlRaw("EXEC dbo.usp_GetEmployees")
                    .ToListAsync();

                return rows
                    .Where(r => r.MunisCode.HasValue)
                    .GroupBy(r => r.MunisCode!.Value)
                    .ToDictionary(g => g.Key, g => g.First().LongevityDate);
            }
            catch
            {
                // Swallow and fall back — consider logging this once the real procedure is wired up.
                return new Dictionary<int, DateTime?>();
            }
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

            var entities = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Current/base rate per employee on this page (separate query + in-memory filter,
            // same Contains()->CTE workaround used elsewhere in this app).
            var employeeIdsOnPage = entities.Select(e => e.ObjectID).ToHashSet();
            var currentRatesByEmployee = (await _context.EmployeeRates
                    .Where(er => !er.Deleted && (er.EndDate == null || er.EndDate >= DateTime.Today))
                    .ToListAsync())
                .Where(er => employeeIdsOnPage.Contains(er.EmployeeObjectId))
                .GroupBy(er => er.EmployeeObjectId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(er => er.StartDate).First());

            var longevityDatesByMunisCode = await GetLongevityDatesFromStoredProcedureAsync();

            // Accounts: all rows (including inactive/deleted) so existing employees still show a name.
            var accountRows = await _context.Accounts
                .Select(a => new { a.ObjectID, a.Code, a.Description, a.Active, a.Deleted })
                .ToListAsync();

            var accountNamesById = accountRows.ToDictionary(
                a => a.ObjectID,
                a => string.IsNullOrWhiteSpace(a.Description) ? a.Code : $"{a.Code} - {a.Description}");

            // Dropdown source for the Add/Edit modals: active, non-deleted accounts only.
            filter.AccountOptions = accountRows
                .Where(a => a.Active == true && a.Deleted != true)
                .OrderBy(a => a.Code)
                .Select(a => new AccountOptionVM { ObjectID = a.ObjectID, Name = accountNamesById[a.ObjectID] })
                .ToList();

            filter.Employees = entities.Select(e =>
            {
                currentRatesByEmployee.TryGetValue(e.ObjectID, out var currentRate);

                DateTime? longevityDate = e.LongevityDate; // fallback
                if (e.MunisCode.HasValue && longevityDatesByMunisCode.TryGetValue(e.MunisCode.Value, out var spDate))
                    longevityDate = spDate;

                string? accountName = null;
                if (e.AccountObjectId.HasValue &&
                    accountNamesById.TryGetValue(e.AccountObjectId.Value, out var name))
                    accountName = name;

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
                    LongevityDate = longevityDate,
                    TitleCode = e.TitleCode,
                    TerminationDate = e.TerminationDate,
                    HireDate = e.HireDate,
                    EmployeeTypeObjectID = e.EmployeeTypeObjectID,
                    AccountObjectId = e.AccountObjectId,
                    Active = e.Active,
                    BaseRate = currentRate?.BaseRate,
                    CurrentRate = currentRate?.Rate,
                    AccountName = accountName
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
            if (!vm.AccountObjectId.HasValue)
                return (false, "Account is required.");

            var accountExists = await _context.Accounts
                .AnyAsync(a => a.ObjectID == vm.AccountObjectId.Value && a.Deleted != true);
            if (!accountExists)
                return (false, "Selected account was not found.");

            var entity = new Employee
            {
                Code = GenerateCodeFromMunisCode(vm.MunisCode.Value),
                MunisCode = vm.MunisCode,
                FirstName = vm.FirstName,
                MI = vm.MI,
                LastName = vm.LastName,
                HireDate = vm.HireDate,
                AccountObjectId = vm.AccountObjectId,
                EnteredByUser = enteredByUser,
                EnteredDate = DateTime.Now,
                Active = true,
                Deleted = false
            };

            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();
            return (true, $"Employee created successfully (Code {entity.Code}).");
        }

        // Edits everything except rate data (rates are only changed through IEmployeeRateService).
        public async Task<(bool Success, string Message)> UpdateAsync(EmployeeVM vm, string enteredByUser)
        {
            var entity = await _context.Employees.FindAsync(vm.ObjectID);
            if (entity == null || entity.Deleted)
                return (false, "Employee not found.");

            if (vm.MunisCode.HasValue)
            {
                entity.MunisCode = vm.MunisCode;
                entity.Code = GenerateCodeFromMunisCode(vm.MunisCode.Value); // keep Code in sync
            }

            entity.FirstName = vm.FirstName;
            entity.MI = vm.MI;
            entity.LastName = vm.LastName;
            if (vm.SSN.HasValue)
                entity.SSN = vm.SSN; // only overwrite if resubmitted; grid never sends full SSN back
            entity.Shift = vm.Shift;
            entity.LongevityRate = vm.LongevityRate;
            // LongevityDate intentionally NOT set here — sourced from the stored procedure.
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