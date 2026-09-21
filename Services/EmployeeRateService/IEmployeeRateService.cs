using CostAccounting.Models;


namespace CostAccounting.Services.EmployeeRateService
{
    public interface IEmployeeRateService
    {
        Task<EmployeeRateIndexVM> GetIndexAsync(EmployeeRateIndexVM filter);
        Task<EmployeeRateVM> GetByIdAsync(int objectId);
        Task<(bool Success, string Message)> CreateAsync(EmployeeRateVM vm, string enteredByUser);
        Task<(bool Success, string Message)> UpdateAsync(EmployeeRateVM vm, string enteredByUser);
        Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
        Task<(bool Success, string Message)> BulkEndDateAsync(List<int> objectIds, System.DateTime endDate);

        Task<(bool Success, string Message)> ApplySeasonalAdjustmentAsync(
            bool isIncrease, System.DateTime effectiveDate, List<int> employeeObjectIds, string enteredByUser);
    }
}
