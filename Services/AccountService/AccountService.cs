using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts.ToListAsync();
        }


        // ── Read ───────────────────────────────────────────────────────────
       public async Task<(IEnumerable<AccountVM> Items, int TotalCount)> GetAccountsAsync(
    string? searchTerm,
    bool? filterActive,
    int? filterTypeId,
    int page,
    int pageSize,
    string? sortColumn = "Code",
    string? sortDirection = "asc")
{
    var query = _context.Accounts
        .Where(a => a.Deleted != true)
        .Join(_context.Lookups,
              a => a.AccountTypeObjectID,
              at => at.ObjectID,
              (a, at) => new AccountVM
              {
                  ObjectID = a.ObjectID,
                  Code = a.Code,
                  Description = a.Description,
                  AccountTypeName = at.Code,
                  AccountTypeId = a.AccountTypeObjectID,
                  Active = a.Active,
                  EnteredDate = a.EnteredDate,
                  EnteredByUser = a.EnteredByUser
              });

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.Trim().ToLower();
        query = query.Where(a =>
            (a.Code != null && a.Code.ToLower().Contains(term)) ||
            (a.Description != null && a.Description.ToLower().Contains(term)) ||
            (a.EnteredByUser != null && a.EnteredByUser.ToLower().Contains(term)));
    }

    if (filterActive.HasValue)
        query = query.Where(a => a.Active == filterActive.Value);

    if (filterTypeId.HasValue)
        query = query.Where(a => a.AccountTypeId == filterTypeId.Value);

    var totalCount = await query.CountAsync();

    bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    query = (sortColumn?.ToLower()) switch
    {
        "description"     => desc ? query.OrderByDescending(a => a.Description)     : query.OrderBy(a => a.Description),
        "accounttypename" => desc ? query.OrderByDescending(a => a.AccountTypeName) : query.OrderBy(a => a.AccountTypeName),
        "enteredbyuser"   => desc ? query.OrderByDescending(a => a.EnteredByUser)   : query.OrderBy(a => a.EnteredByUser),
        "entereddate"     => desc ? query.OrderByDescending(a => a.EnteredDate)     : query.OrderBy(a => a.EnteredDate),
        "active"          => desc ? query.OrderByDescending(a => a.Active)          : query.OrderBy(a => a.Active),
        _                 => desc ? query.OrderByDescending(a => a.Code)            : query.OrderBy(a => a.Code),
    };

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, totalCount);
}
        public async Task<IEnumerable<Lookup>> GetAccountTypesAsync()
        {
            return await _context.Lookups
                .Where(l => l.LookupCategoryObjectID == 16)
                .OrderBy(l => l.Code)
                .ToListAsync();
        }



        // ── Create ─────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> CreateAccountAsync(AccountCreateViewModel model)
        {
            // Duplicate code check
            var exists = await _context.Accounts
                .AnyAsync(a => a.Code == model.Code && a.Deleted != true);

            if (exists)
                return (false, $"An account with code '{model.Code}' already exists.");

            var account = new Account
            {
                Code = model.Code?.Trim(),
                Description = model.Description?.Trim(),
                AccountTypeObjectID = model.AccountTypeObjectID,   // stores 88/89/90
                AgencyTypeObjectID = model.AgencyTypeObjectID,
                Active = model.Active,
                Deleted = false,
                EnteredDate = DateTime.Now,
                EnteredByUser = model.EnteredByUser?.Trim()
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return (true, "Account created successfully.");
        }

        // ── Update (inline edit) ───────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateAccountAsync(AccountInlineEditViewModel model)
        {
            var account = await _context.Accounts.FindAsync(model.ObjectID);
            if (account == null)
                return (false, "Account not found.");

            // Duplicate code check (exclude self)
            var exists = await _context.Accounts
                .AnyAsync(a => a.Code == model.Code &&
                               a.ObjectID != model.ObjectID &&
                               a.Deleted != true);

            if (exists)
                return (false, $"Another account with code '{model.Code}' already exists.");

            account.Code = model.Code?.Trim();
            account.Description = model.Description?.Trim();
            account.AccountTypeObjectID = model.AccountTypeObjectID;
            account.AgencyTypeObjectID = model.AgencyTypeObjectID;
            account.Active = model.Active;
            account.EnteredByUser = model.EnteredByUser?.Trim();

            await _context.SaveChangesAsync();
            return (true, "Account updated.");
        }

        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var account = await _context.Accounts.FindAsync(objectId);
            if (account == null)
                return (false, "Record not found.");

            try
            {
                account.Deleted = true;
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
