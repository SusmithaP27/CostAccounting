using CostAccounting.Models;

namespace CostAccounting.Services
{
    public interface IRoadService
    {
        Task<RoadVM> GetPagedRoadsAsync(
    string? searchTerm,
    bool? filterActive,
    bool? filterEngineering,
    int page,
    int pageSize,
    string? sortColumn = "Code",
    string? sortDirection = "asc");

        Task<Road?> GetByIdAsync(short objectId);

        Task<(bool Success, string Message)> CreateAsync(Road road);

        Task<(bool Success, string Message)> UpdateInlineAsync(short objectId, bool? active, bool? engineering);

        Task<(bool Success, string Message)> SoftDeleteAsync(short objectId);

    }
}
