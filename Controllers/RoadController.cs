using CostAccounting.Models;
using CostAccounting.Services;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class RoadController : Controller
    {
        private readonly IRoadService _roadService;

        public RoadController(IRoadService roadService)
        {
            _roadService = roadService;
        }

        // GET: /Roads
        public async Task<IActionResult> Index(
            string? searchTerm,
            bool? filterActive,
            bool? filterEngineering,
            string sortColumn = "Code",
            string sortDirection = "asc",
            int page = 1,
            int pageSize = 15)
        {
            // True first visit (no querystring at all) → default Active to "Yes"
            bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                              && !Request.Query.ContainsKey("searchTerm")
                              && !Request.Query.ContainsKey("filterEngineering")
                              && !Request.Query.ContainsKey("sortColumn");

            if (isFirstVisit)
                filterActive = true;

            var viewModel = await _roadService.GetPagedRoadsAsync(
                searchTerm, filterActive, filterEngineering, page, pageSize, sortColumn, sortDirection);

            return View(viewModel);
        }

        // POST: /Roads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Description,EnteredByUser")] Road road)
        {
            ModelState.Remove("ObjectID");
            ModelState.Remove("RoadTypeObjectID");
            ModelState.Remove("EnteredDate");
            ModelState.Remove("Active");
            ModelState.Remove("Deleted");
            ModelState.Remove("Engineering");

            road.EnteredByUser = User.Identity?.Name ?? "system";

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var (success, message) = await _roadService.CreateAsync(road);
            return Json(new { success, message });
        }

        // POST: /Roads/InlineEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineEdit(short objectId, bool? active, bool? engineering)
        {
            var (success, message) = await _roadService.UpdateInlineAsync(objectId, active, engineering);
            return Json(new { success, message });
        }

        // POST: /Roads/SoftDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(short id)
        {
            var (success, message) = await _roadService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }
    }
}