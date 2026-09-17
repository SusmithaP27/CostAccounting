using CostAccounting.Models;
using CostAccounting.Models.ViewModels;

namespace CostAccounting.Services
{
    public interface IAllowanceService
    {
        Task<AllowanceIndexVM> GetAllowancesAsync(
            int? filterYear, bool? filterActive, int? filterSubTypeId,
            string sortColumn, string sortDirection, int page, int pageSize);

        Task<List<Lookup>> GetSubTypesAsync();

        Task<(bool Success, string Message)> CreateAllowanceAsync(
            short? year, decimal? rate, int? subTypeId, bool active, string enteredByUser);

        Task<(bool Success, string Message)> UpdateAllowanceAsync(
            int objectId, short? year, decimal? rate, int? subTypeId, bool active);

        Task<(bool Success, string Message)> DeleteAllowancesAsync(List<int> objectIds);
    }
}