using CostAccounting.Helpers;
using CostAccounting.Models;
using CostAccounting.Services.MaterialRateService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class MaterialRateController : Controller
    {
        private readonly IMaterialRateService _svc;

        public MaterialRateController(IMaterialRateService svc) => _svc = svc;

        [HttpGet]
public async Task<IActionResult> Index(
    int? filterMaterialId,
    bool showAllMaterials = true,
    string? startDateOperator = "<=",
    DateTime? startDateFilter = null,
    string? endDateOperator = ">=",
    DateTime? endDateFilter = null,
    bool showAllDates = true,
    string? searchTerm = null,
    string sortColumn = "MaterialCode",
    string sortDir = "asc",
    int page = 1,
    int pageSize = 15)
{
    bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    if (page < 1) page = 1;
    pageSize = PagingDefaults.Normalize(pageSize);
    var (items, total) = await _svc.GetRatesAsync(
        filterMaterialId, showAllMaterials,
        startDateOperator, startDateFilter,
        endDateOperator, endDateFilter,
        showAllDates, searchTerm,
        sortColumn, sortDir, page, pageSize);

    // Rows were deleted and this page is now past the end: step back
    int totalPages = (int)Math.Ceiling(total / (double)pageSize);
    if (total > 0 && page > totalPages)
    {
        page = totalPages;
        (items, total) = await _svc.GetRatesAsync(
            filterMaterialId, showAllMaterials,
            startDateOperator, startDateFilter,
            endDateOperator, endDateFilter,
            showAllDates, searchTerm,
            sortColumn, sortDir, page, pageSize);
    }

    var vm = new MaterialRateIndexVM
    {
        Rates = items,
        Materials = await _svc.GetMaterialsAsync(),
        FilterMaterialId = filterMaterialId,
        ShowAllMaterials = showAllMaterials,
        StartDateOperator = startDateOperator,
        StartDateFilter = startDateFilter,
        EndDateOperator = endDateOperator,
        EndDateFilter = endDateFilter,
        ShowAllDates = showAllDates,
        SearchTerm = searchTerm,
        SortColumn = sortColumn,
        SortDir = sortDir,
        CurrentPage = page,
        PageSize = pageSize,
        TotalCount = total
    };

    return isAjax ? PartialView("_RatesTable", vm) : View(vm);
}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int materialObjectId, DateTime? startDate, DateTime? endDate, decimal rate)
        {
            if (materialObjectId <= 0) return Json(new { success = false, message = "Select a material." });
            if (rate <= 0) return Json(new { success = false, message = "Rate must be greater than 0." });
            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
                return Json(new { success = false, message = "End date cannot be before start date." });

            await _svc.CreateAsync(new MaterialRateVM
            {
                MaterialObjectId = materialObjectId,
                StartDate = startDate,
                EndDate = endDate,
                Rate = rate
            });
            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRate(int objectId, decimal rate, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
                return Json(new { success = false, message = "End date cannot be before start date." });

            var (ok, msg) = await _svc.UpdateRateAsync(objectId, rate, startDate, endDate);
            return Json(new { success = ok, message = msg });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateEndDate(string selectedIds, DateTime newEndDate)
        {
            var (ok, msg) = await _svc.BulkUpdateEndDateAsync(ParseIds(selectedIds), newEndDate);
            return Json(new { success = ok, message = msg });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(string selectedIds)
        {
            var (ok, msg) = await _svc.DeleteAsync(ParseIds(selectedIds));
            return Json(new { success = ok, message = msg });
        }

        private static List<int> ParseIds(string? csv) =>
            (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Select(s => int.TryParse(s, out var i) ? i : 0)
                       .Where(i => i > 0)
                       .ToList();
    }
}
