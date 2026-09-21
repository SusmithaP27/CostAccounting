using CostAccounting.Models;

namespace CostAccounting.Services.EmployeeService
{
    public interface IEmployeeService
    {
        // Task<(IEnumerable<EmployeeVM> Items, int TotalCount)> GetEmployeesAsync(
        //     string? searchTerm,
        //     string? sortColumn,
        //     string? sortDir,
        //     int page,
        //     int pageSize);
        Task<EmployeeIndexVM> GetIndexAsync(EmployeeIndexVM filter);
        Task<EmployeeVM> GetByIdAsync(int objectId);
        Task<(bool Success, string Message)> CreateAsync(EmployeeVM vm, string enteredByUser);
        Task<(bool Success, string Message)> UpdateAsync(EmployeeVM vm, string enteredByUser);
        Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
        Task<List<EmployeeOptionVM>> GetActiveEmployeeOptionsAsync();
    
    }
}
