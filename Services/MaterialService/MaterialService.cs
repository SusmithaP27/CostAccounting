using CostAccounting.Data;
using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;


namespace CostAccounting.Services.MaterialService
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;

        public MaterialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Material>> GetAllAsync()
        {
            return await _context.Materials.ToListAsync();
        }

        public async Task<(IEnumerable<MaterialVM> Items, int TotalCount)> GetMaterialsAsync(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string? sortColumn,
            string? sortDir,
            int page,
            int pageSize)
        {
            var query =
                from m in _context.Materials

                join l in _context.Lookups
                    on m.UnitTypeObjectID equals l.ObjectID into lookupGroup
                from l in lookupGroup.DefaultIfEmpty()

                join mr in _context.MaterialsRate.Where(x =>
                        x.Deleted != true &&
                        x.StartDate <= DateTime.Now &&
                        x.EndDate >= DateTime.Now)
                    on m.ObjectID equals mr.MaterialObjectId into rateGroup
                from mr in rateGroup.DefaultIfEmpty()

                where m.Deleted != true
                      && m.ObjectID > 0
                      && (l == null || l.Active == true)

                select new MaterialVM
                {
                    ObjectID = m.ObjectID,
                    Code = m.Code,
                    Description = m.Description,
                    EffectiveDate = m.EffectiveDate,
                    VendorName = m.VendorName,
                    Contract = m.ContractNum,

                    UnitTypeObjectID = m.UnitTypeObjectID,

                    Rate = mr != null ? mr.Rate : 0,

                    Unit = l != null ? l.Description : "",

                    Active = m.Active
                };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();

                query = query.Where(a =>
                    (a.Code != null && a.Code.ToLower().Contains(term)) ||
                    (a.Description != null && a.Description.ToLower().Contains(term)) ||
                    (a.VendorName != null && a.VendorName.ToLower().Contains(term)) ||
                    (a.Contract != null && a.Contract.ToLower().Contains(term)));
            }

            if (filterActive.HasValue)
                query = query.Where(a => a.Active == filterActive.Value);

            if (filterTypeId.HasValue)
                query = query.Where(a => a.UnitTypeObjectID == filterTypeId.Value);

            var totalCount = await query.CountAsync();

            bool asc = sortDir?.ToLower() != "desc";

            query = sortColumn?.ToLower() switch
            {
                "description" => asc ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description),
                "effectivedate" => asc ? query.OrderBy(x => x.EffectiveDate) : query.OrderByDescending(x => x.EffectiveDate),
                "vendorname" => asc ? query.OrderBy(x => x.VendorName) : query.OrderByDescending(x => x.VendorName),
                "contract" => asc ? query.OrderBy(x => x.Contract) : query.OrderByDescending(x => x.Contract),
                "unit" => asc ? query.OrderBy(x => x.Unit) : query.OrderByDescending(x => x.Unit),
                "rate" => asc ? query.OrderBy(x => x.Rate) : query.OrderByDescending(x => x.Rate),
                "active" => asc ? query.OrderBy(x => x.Active) : query.OrderByDescending(x => x.Active),
                _ => asc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Lookup>> GetUnitTypesAsync()
        {
            return await _context.Lookups
                .Where(l => l.LookupCategoryObjectID == 15)
                .OrderBy(l => l.Code)
                .ToListAsync();
        }

        public async Task<MaterialVM> CreateAsync(MaterialVM model)
        {
            var entity = new Material
            {
                EnteredDate = DateTime.UtcNow,
                Code = model.Code,
                Description = model.Description,
                EffectiveDate = model.EffectiveDate,
                VendorName = model.VendorName,
                ContractNum = model.Contract,
                UnitTypeObjectID = model.UnitTypeObjectID,
                Active = model.Active ?? true,
                Deleted = false
            };

            _context.Materials.Add(entity);
            await _context.SaveChangesAsync();

            model.ObjectID = entity.ObjectID;
            return model;
        }

        public async Task<MaterialVM?> UpdateAsync(MaterialVM model)
        {
            var entity = await _context.Materials.FindAsync(model.ObjectID);
            if (entity == null || entity.Deleted == true) return null;

            entity.Description = model.Description;
            entity.EffectiveDate = model.EffectiveDate;
            entity.VendorName = model.VendorName;
            entity.ContractNum = model.Contract;
            entity.UnitTypeObjectID = model.UnitTypeObjectID;
            entity.Active = model.Active;

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Materials.FindAsync(id);
            if (entity == null) return false;

            entity.Deleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}