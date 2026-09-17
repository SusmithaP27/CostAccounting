using CostAccounting.Models;

namespace CostAccounting.Services.EquipmentRateService
{
    public interface IEquipmentRateService
    {
        Task<IEnumerable<EquipmentLookupVM>> GetEquipmentListAsync();

        Task<(IEnumerable<EquipmentRateVM> Items, int TotalCount)> GetRatesAsync(
            int? filterEquipmentId,
            bool showAllEquipment,
            string? startDateOperator,
            DateTime? startDateFilter,
            string? endDateOperator,
            DateTime? endDateFilter,
            bool showAllDates,
            string? searchTerm,
            int page,
            int pageSize);

        Task<EquipmentRateVM> CreateAsync(EquipmentRateVM model);
        Task<EquipmentRateVM?> UpdateAsync(EquipmentRateVM model);
        Task<int> BulkUpdateEndDateAsync(List<int> objectIds, DateTime newEndDate);
        Task<int> BulkDeleteAsync(List<int> objectIds);
    }
}
