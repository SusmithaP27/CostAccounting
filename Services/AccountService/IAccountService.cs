using CostAccounting.Models;

namespace CostAccounting.Services.AccountService
{
    public interface IAccountService
    {
        Task<List<Account>> GetAllAsync();

        // ── Read ───────────────────────────────────────────────────────────
        Task<(IEnumerable<AccountVM> Items, int TotalCount)> GetAccountsAsync(
    string? searchTerm,
    bool? filterActive,
    int? filterTypeId,
    int page,
    int pageSize,
    string? sortColumn = "Code",
    string? sortDirection = "asc");

        Task<IEnumerable<Lookup>> GetAccountTypesAsync();

        // ── Write ──────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> CreateAccountAsync(AccountCreateViewModel model);

        Task<(bool Success, string Message)> UpdateAccountAsync(AccountInlineEditViewModel model);

        Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
    }
}
