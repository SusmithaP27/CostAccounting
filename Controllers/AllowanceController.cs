using CostAccounting.Services;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Helpers;

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
            string sortColumn = "Year", string sortDirection = "desc", int page = 1,int pageSize = 15)
        {
            if (page < 1) page = 1;
            pageSize = PagingDefaults.Normalize(pageSize);

            bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                              && !Request.Query.ContainsKey("filterYear")
                              && !Request.Query.ContainsKey("filterSubTypeId")
                              && !Request.Query.ContainsKey("sortColumn");

            if (isFirstVisit)
                filterActive = true;

            var vm = await _allowanceService.GetAllowancesAsync(
                filterYear, filterActive, filterSubTypeId, sortColumn, sortDirection, page, pageSize);

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
public async Task<IActionResult> DeleteAllowance(int id)
{
    var result = await _allowanceService.DeleteAllowanceAsync(id);
    return Json(new { success = result.Success, message = result.Message });
}
    }
}