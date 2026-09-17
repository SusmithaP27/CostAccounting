using CostAccounting.Models;

namespace CostAccounting.Services.MaterialService
{
    public interface IMaterialService
    {
        Task<List<Material>> GetAllAsync();

        Task<(IEnumerable<MaterialVM> Items, int TotalCount)> GetMaterialsAsync(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string? sortColumn,
            string? sortDir,
            int page,
            int pageSize);

        Task<IEnumerable<Lookup>> GetUnitTypesAsync();
        Task<MaterialVM> CreateAsync(MaterialVM model);
        Task<MaterialVM?> UpdateAsync(MaterialVM model);
        Task<bool> SoftDeleteAsync(int id);

    }
}
