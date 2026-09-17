using CostAccounting.Models;

namespace CostAccounting.Services.OperationService
{
    public interface IOperationService
    {
        Task<OperationVM> GetPagedOperationsAsync(
    string? searchTerm,
    bool? filterActive,
    bool? filterEngineering,
    int page,
    int pageSize,
    string? sortColumn = "Code",
    string? sortDirection = "asc");

        Task<Operation?> GetByIdAsync(short objectId);

        Task<(bool Success, string Message)> CreateAsync(Operation operation);

        Task<(bool Success, string Message)> UpdateInlineAsync(short objectId, bool? active, bool? engineering);

        Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
    }
}
