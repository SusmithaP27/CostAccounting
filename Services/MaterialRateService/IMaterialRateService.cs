using CostAccounting.Models;

namespace CostAccounting.Services.MaterialRateService
{
    public interface IMaterialRateService
    {
        Task<(IEnumerable<MaterialRateVM> Items, int TotalCount)> GetRatesAsync(
            int? filterMaterialId,
            bool showAllMaterials,
            bool showAllDates,
            DateTime? startDate,
            DateTime? endDate,
            string? searchTerm,
            string? sortColumn,
            string? sortDir,
            int page,
            int pageSize);

        Task<IEnumerable<Material>> GetMaterialsAsync();

        Task<MaterialRateVM> CreateAsync(MaterialRateVM model);

        // Bulk-update end date for selected rate rows
        Task<(bool Success, string Message)> BulkUpdateEndDateAsync(
            IEnumerable<int> objectIds,
            DateTime newEndDate);

        // Single row inline rate edit
        Task<(bool Success, string Message)> UpdateRateAsync(
            int objectId,
            decimal rate,
            DateTime? startDate,
            DateTime? endDate);
    }
}