//using CostAccounting.Data;
//using CostAccounting.Models;
//using Microsoft.EntityFrameworkCore;

//namespace CostAccounting.Services.EquipmentService
//{
//    public class EquipmentService : IEquipmentService
//    {
//        private readonly ApplicationDbContext _context;

//        public EquipmentService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<Equipment>> GetAllAsync()
//        {
//            return await _context.Equipments.ToListAsync();
//        }

//        public async Task<(IEnumerable<EquipmentVM> Items, int TotalCount)> GetEquipmentsAsync(
//            string? searchTerm,
//            bool? filterActive,
//            int? filterTypeId,
//            string? sortColumn,
//            string? sortDir,
//            int page,
//            int pageSize)
//        {
//            var query = _context.Equipments
//                .Where(e => e.Deleted != true)
//                .GroupJoin(_context.EquipmentsRate,
//                           e => e.ObjectID,
//                           r => r.ObjectID,
//                           (e, rates) => new { e, rates })
//                .SelectMany(
//                    x => x.rates.DefaultIfEmpty(),
//                    (x, r) => new EquipmentVM
//                    {
//                        ObjectID = x.e.ObjectID,
//                        Code = x.e.Code,
//                        Category = x.e.Category,
//                        MfgYear = x.e.MfgYear,
//                        // Format DateTime? → string for the VM
//                        DateInService = x.e.DateInService != null
//                                        ? x.e.DateInService.Value.ToString("yyyy-MM-dd")
//                                        : null,
//                        Make = x.e.Make,
//                        Model = x.e.Model,
//                        Department = x.e.Department,
//                        CurrentRate = r.Rate,
//                        Active = x.e.Active,
//                    });

//            if (!string.IsNullOrWhiteSpace(searchTerm))
//            {
//                var term = searchTerm.Trim().ToLower();
//                query = query.Where(a =>
//                    (a.Code != null && a.Code.ToLower().Contains(term)) ||
//                    (a.Category != null && a.Category.ToLower().Contains(term)) ||
//                    (a.Make != null && a.Make.ToLower().Contains(term)) ||
//                    (a.Department != null && a.Department.ToLower().Contains(term)) ||
//                    (a.Model != null && a.Model.ToLower().Contains(term)));
//            }

//            if (filterActive.HasValue)
//                query = query.Where(a => a.Active == filterActive.Value);

//            var totalCount = await query.CountAsync();

//            bool asc = sortDir?.ToLower() != "desc";
//            query = sortColumn?.ToLower() switch
//            {
//                "code" => asc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
//                "dateinservice" => asc ? query.OrderBy(x => x.DateInService) : query.OrderByDescending(x => x.DateInService),
//                "make" => asc ? query.OrderBy(x => x.Make) : query.OrderByDescending(x => x.Make),
//                "model" => asc ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model),
//                "category" => asc ? query.OrderBy(x => x.Category) : query.OrderByDescending(x => x.Category),
//                "mfgyear" => asc ? query.OrderBy(x => x.MfgYear) : query.OrderByDescending(x => x.MfgYear),
//                "department" => asc ? query.OrderBy(x => x.Department) : query.OrderByDescending(x => x.Department),
//                "active" => asc ? query.OrderBy(x => x.Active) : query.OrderByDescending(x => x.Active),
//                _ => asc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
//            };

//            var items = await query
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();

//            return (items, totalCount);
//        }

//        public async Task<EquipmentVM> CreateAsync(EquipmentVM model)
//        {
//            var entity = new Equipment
//            {
//                EnteredDate = DateTime.UtcNow,
//                Code = model.Code,
//                Category = model.Category,
//                MfgYear = model.MfgYear,
//                DateInService = DateTime.TryParse(model.DateInService, out var d) ? d : null,
//                Make = model.Make,
//                Model = model.Model,
//                Department = model.Department,
//                Active = model.Active ?? true,
//                Deleted = false
//            };

//            _context.Equipments.Add(entity);
//            await _context.SaveChangesAsync();

//            model.ObjectID = entity.ObjectID;
//            return model;
//        }

//        public async Task<EquipmentVM?> UpdateAsync(EquipmentVM model)
//        {
//            var entity = await _context.Equipments.FindAsync(model.ObjectID);
//            if (entity == null || entity.Deleted == true) return null;

//            entity.Category = model.Category;
//            entity.MfgYear = model.MfgYear;
//            entity.DateInService = DateTime.TryParse(model.DateInService, out var d) ? d : null;
//            entity.Make = model.Make;
//            entity.Model = model.Model;
//            entity.Department = model.Department;
//            entity.Active = model.Active;

//            await _context.SaveChangesAsync();
//            return model;
//        }

//        public async Task<Equipment?> GetByIdAsync(int objectID)
//        {
//            return await _context.Equipments
//                .FirstOrDefaultAsync(e => e.ObjectID == objectID && e.Deleted != true);
//        }

//        public async Task DeleteAsync(int objectID)
//        {
//            var entity = await _context.Equipments.FindAsync(objectID);
//            if (entity == null) return;
//            entity.Deleted = true;
//            await _context.SaveChangesAsync();
//        }
//    }
//}
using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.EquipmentService
{
    public class EquipmentService : IEquipmentService
    {
        private readonly ApplicationDbContext _context;

        public EquipmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Equipment>> GetAllAsync()
        {
            return await _context.Equipments.ToListAsync();
        }

        public async Task<(IEnumerable<EquipmentVM> Items, int TotalCount)> GetEquipmentsAsync(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string? sortColumn,
            string? sortDir,
            int page,
            int pageSize)
        {
            var query =
                from e in _context.Equipments
                where e.Deleted != true

                join r in _context.EquipmentsRate
                    on e.ObjectID equals r.ObjectID into rateGroup

                from r in rateGroup.DefaultIfEmpty()

                select new EquipmentVM
                {
                    ObjectID = e.ObjectID,
                    Code = e.Code,
                    Category = e.Category,
                    MfgYear = e.MfgYear,
                    DateInService = e.DateInService,
                    Make = e.Make,
                    EquipmentModel = e.Model,
                    Department = e.Department,
                    CurrentRate = r != null ? r.Rate : null,
                    Active = e.Active
                };

            // ── Search ──────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a =>
                    (a.Code != null && a.Code.ToLower().Contains(term)) ||
                    (a.Category != null && a.Category.ToLower().Contains(term)) ||
                    (a.Make != null && a.Make.ToLower().Contains(term)) ||
                    (a.EquipmentModel != null && a.EquipmentModel.ToLower().Contains(term)) ||
                    (a.Department != null && a.Department.ToLower().Contains(term)));
            }

            // ── Filters ─────────────────────────────────────────────────────
            if (filterActive.HasValue)
                query = query.Where(a => a.Active == filterActive.Value);

            // ── Total count before paging ────────────────────────────────────
            var totalCount = await query.CountAsync();

            // ── Sort ─────────────────────────────────────────────────────────
            bool asc = sortDir?.ToLower() != "desc";
            query = sortColumn?.ToLower() switch
            {
                "code" => asc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "category" => asc ? query.OrderBy(x => x.Category) : query.OrderByDescending(x => x.Category),
                "mfgyear" => asc ? query.OrderBy(x => x.MfgYear) : query.OrderByDescending(x => x.MfgYear),
                "dateinservice" => asc ? query.OrderBy(x => x.DateInService) : query.OrderByDescending(x => x.DateInService),
                "make" => asc ? query.OrderBy(x => x.Make) : query.OrderByDescending(x => x.Make),
                "model" => asc ? query.OrderBy(x => x.EquipmentModel) : query.OrderByDescending(x => x.EquipmentModel),
                "department" => asc ? query.OrderBy(x => x.Department) : query.OrderByDescending(x => x.Department),
                "active" => asc ? query.OrderBy(x => x.Active) : query.OrderByDescending(x => x.Active),
                _ => asc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            };

            // ── Page ─────────────────────────────────────────────────────────
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<EquipmentVM> CreateAsync(EquipmentVM model)
        {
            Console.WriteLine("**************************************************");
            Console.WriteLine(model.Code);
            var entity = new Equipment
            {
                EnteredDate = DateTime.UtcNow,
                Code = model.Code,
                Category = model.Category,
                MfgYear = model.MfgYear,
                DateInService = model.DateInService,
                Make = model.Make,
                Model = model.EquipmentModel,
                Department = model.Department,
                Active = model.Active ?? true,
                Deleted = false
            };

            _context.Equipments.Add(entity);
            await _context.SaveChangesAsync();

            model.ObjectID = entity.ObjectID;
            return model;
        }

        public async Task<EquipmentVM?> UpdateAsync(EquipmentVM model)
        {
            var entity = await _context.Equipments.FindAsync(model.ObjectID);
            if (entity == null || entity.Deleted == true) return null;

            entity.Category = model.Category;
            entity.MfgYear = model.MfgYear;
            entity.DateInService = model.DateInService;
            entity.Make = model.Make;
            entity.Model = model.EquipmentModel;
            entity.Department = model.Department;
            entity.Active = model.Active;

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Equipments.FindAsync(id);
            if (entity == null) return false;

            entity.Deleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}