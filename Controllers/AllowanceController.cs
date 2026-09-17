using CostAccounting.Services;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class AllowanceController : Controller
    {
        private readonly IAllowanceService _allowanceService;

        public AllowanceController(IAllowanceService allowanceService)
        {
            _allowanceService = allowanceService;
        }

        public async Task<IActionResult> Index(
            int? filterYear, bool? filterActive, int? filterSubTypeId,
            string sortColumn = "Year", string sortDirection = "desc", int page = 1)
        {
            bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                              && !Request.Query.ContainsKey("filterYear")
                              && !Request.Query.ContainsKey("filterSubTypeId")
                              && !Request.Query.ContainsKey("sortColumn");

            if (isFirstVisit)
                filterActive = true;

            var vm = await _allowanceService.GetAllowancesAsync(
                filterYear, filterActive, filterSubTypeId, sortColumn, sortDirection, page, 15);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(short? Year, decimal? Rate, int? AllowanceSubTypeObjectID,
            bool Active, string EnteredByUser)
        {
            ModelState.Clear();
            var (success, message) = await _allowanceService.CreateAllowanceAsync(
                Year, Rate, AllowanceSubTypeObjectID, Active,
                string.IsNullOrWhiteSpace(EnteredByUser) ? (User.Identity?.Name ?? "system") : EnteredByUser);

            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineEdit(int ObjectID, short? Year, decimal? Rate,
            int? AllowanceSubTypeObjectID, bool Active)
        {
            ModelState.Clear();
            var (success, message) = await _allowanceService.UpdateAllowanceAsync(
                ObjectID, Year, Rate, AllowanceSubTypeObjectID, Active);

            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] List<int> objectIds)
        {
            ModelState.Clear();
            var (success, message) = await _allowanceService.DeleteAllowancesAsync(objectIds);
            return Json(new { success, message });
        }
    }
}