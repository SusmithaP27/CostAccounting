using CostAccounting.Models;
using CostAccounting.Services.MaterialRateService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class MaterialRateController : Controller
    {
        private readonly IMaterialRateService _rateService;

        public MaterialRateController(IMaterialRateService rateService)
        {
            _rateService = rateService;
        }

        // GET: /MaterialRates

        //[HttpGet]
        public async Task<IActionResult> Index(
            int? filterMaterialId = null,
            bool showAllMaterials = true,
            bool showAllDates = true,
            DateTime? filterStartDate = null,
            DateTime? filterEndDate = null,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortDir = null,
            int page = 1,
            int pageSize = 20)
        {
            var start = filterStartDate ?? DateTime.Today;
            var end = filterEndDate ?? DateTime.Today;

            var (rates, totalCount) = await _rateService.GetRatesAsync(
                filterMaterialId, showAllMaterials, showAllDates,
                start, end, searchTerm,
                sortColumn, sortDir,
                page, pageSize);

            var materials = await _rateService.GetMaterialsAsync();

            var vm = new MaterialRateIndexVM
            {
                Rates = rates,
                Materials = materials,
                FilterMaterialId = filterMaterialId,
                ShowAllMaterials = showAllMaterials,
                ShowAllDates = showAllDates,
                FilterStartDate = start,
                FilterEndDate = end,
                SearchTerm = searchTerm,
                SortColumn = sortColumn ?? "MaterialCode",
                SortDir = sortDir ?? "asc",
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_RatesTable", vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialRateVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var created = await _rateService.CreateAsync(model);
            return Json(new { success = true, message = "MaterialRates created.", data = created });
        }

        // POST: /MaterialRates/BulkUpdateEndDate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateEndDate(string selectedIds, DateTime newEndDate)
        {
            if (string.IsNullOrWhiteSpace(selectedIds))
                return Json(new { success = false, message = "No rows selected." });

            var ids = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n > 0);

            var (success, message) = await _rateService.BulkUpdateEndDateAsync(ids, newEndDate);
            return Json(new { success, message });
        }

        // POST: /MaterialRates/UpdateRate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRate(
            int objectId, decimal rate, DateTime? startDate, DateTime? endDate)
        {
            var (success, message) = await _rateService.UpdateRateAsync(objectId, rate, startDate, endDate);
            return Json(new { success, message });
        }
    }
}
