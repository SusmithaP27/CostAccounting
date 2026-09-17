using CostAccounting.Models;
using CostAccounting.Services.MaterialService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class MaterialController : Controller
    {
        private readonly IMaterialService _service;

        public MaterialController(IMaterialService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string? sortColumn,
            string? sortDir,
            int page = 1,
            int pageSize = 15)
        {
            bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                              && !Request.Query.ContainsKey("searchTerm")
                              && !Request.Query.ContainsKey("filterTypeId")
                              && !Request.Query.ContainsKey("sortColumn");

            if (isFirstVisit)
                filterActive = true;

            var (materials, totalCount) = await _service.GetMaterialsAsync(
                searchTerm, filterActive, filterTypeId,
                sortColumn, sortDir,
                page, pageSize);

            var unitTypes = await _service.GetUnitTypesAsync();

            var vm = new MaterialIndexVM
            {
                Materials = materials,
                UnitTypes = unitTypes,
                SearchTerm = searchTerm,
                FilterActive = filterActive,
                FilterTypeId = filterTypeId,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            ViewBag.SortColumn = sortColumn ?? "Code";
            ViewBag.SortDir = sortDir ?? "asc";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_MaterialsTable", vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var created = await _service.CreateAsync(model);
            return Json(new { success = true, message = "Material created.", data = created });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MaterialVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var updated = await _service.UpdateAsync(model);
            if (updated == null)
                return Json(new { success = false, message = "Material not found." });

            return Json(new { success = true, message = "Saved successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.SoftDeleteAsync(id);
            if (!ok)
                return Json(new { success = false, message = "Material not found." });

            return Json(new { success = true, message = "Material deleted." });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitTypes()
        {
            var types = await _service.GetUnitTypesAsync();
            return Ok(types.Select(t => new { t.ObjectID, t.Code }));
        }

    }
}