using CostAccounting.Models;
using CostAccounting.Services.OperationService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class OperationController : Controller
    {
        private readonly IOperationService _operationService;

        public OperationController(IOperationService operationService)
        {
            _operationService = operationService;
        }

        // GET: /Operations
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

            var viewModel = await _operationService.GetPagedOperationsAsync(
                searchTerm, filterActive, filterEngineering, page, pageSize, sortColumn, sortDirection);

            return View(viewModel);
        }

        // POST: /Operations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Description")] Operation operation)
        {
            ModelState.Remove("ObjectID");
            ModelState.Remove("RoadTypeObjectID");
            ModelState.Remove("EnteredDate");
            ModelState.Remove("Active");
            ModelState.Remove("Deleted");
            ModelState.Remove("Engineering");
            ModelState.Remove("EnteredByUser");

            operation.EnteredByUser = User.Identity?.Name ?? "system";

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var (success, message) = await _operationService.CreateAsync(operation);
            return Json(new { success, message });
        }

        // POST: /Operations/InlineEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineEdit(short objectId, bool? active, bool? engineering)
        {
            var (success, message) = await _operationService.UpdateInlineAsync(objectId, active, engineering);
            return Json(new { success, message });
        }

        // POST: /Operations/SoftDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var (success, message) = await _operationService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }
    }
}