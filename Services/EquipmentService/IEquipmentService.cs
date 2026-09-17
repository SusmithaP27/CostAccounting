//using CostAccounting.Models;

//namespace CostAccounting.Services.EquipmentService
//{
//    public interface IEquipmentService
//    {
//        Task<List<Equipment>> GetAllAsync();
//        Task<(IEnumerable<EquipmentVM> Items, int TotalCount)> GetEquipmentsAsync(
//            string? searchTerm, bool? filterActive, int? filterTypeId,
//            string? sortColumn, string? sortDir, int page, int pageSize);
//        Task<Equipment?> GetByIdAsync(int objectID);
//        Task<EquipmentVM> CreateAsync(EquipmentVM model);
//        Task<EquipmentVM?> UpdateAsync(EquipmentVM model);
//        Task DeleteAsync(int objectID);
//    }
//}
using CostAccounting.Models;

namespace CostAccounting.Services.EquipmentService
{
    public interface IEquipmentService
    {
        Task<List<Equipment>> GetAllAsync();
        Task<(IEnumerable<EquipmentVM> Items, int TotalCount)> GetEquipmentsAsync(
            string? searchTerm, bool? filterActive, int? filterTypeId,
            string? sortColumn, string? sortDir, int page, int pageSize);
        Task<EquipmentVM> CreateAsync(EquipmentVM model);
        Task<EquipmentVM?> UpdateAsync(EquipmentVM model);
        Task<bool> SoftDeleteAsync(int id);
    }
}