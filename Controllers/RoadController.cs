using CostAccounting.Models;
using CostAccounting.Services;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Helpers;
using CostAccounting.Services.Reports;

namespace CostAccounting.Controllers
{
    public class RoadController : Controller
    {
        private readonly IRoadService _roadService;
        private readonly IPdfReportService _pdfReport;

        public RoadController(IRoadService roadService, IPdfReportService pdfReport)
        {
            _roadService = roadService;
            _pdfReport = pdfReport;
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
            pageSize = PagingDefaults.Normalize(pageSize);
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

        // Same filters and sort as the grid, but no paging (page 1, every row).
        // Do NOT run PagingDefaults.Normalize on the size here: int.MaxValue is intentional.
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            string? searchTerm,
            bool? filterActive,
            bool? filterEngineering,
            string sortColumn = "Code",
            string sortDirection = "asc")
        {
            var vm = await _roadService.GetPagedRoadsAsync(
                searchTerm, filterActive, filterEngineering, 1, int.MaxValue, sortColumn, sortDirection);
 
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
 
            var def = new ReportDefinition<Road>
            {
                Title = "Road Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Roads",
                Rows = vm.Roads,
                Parameters =
                {
                    ("Active", YesNoAll(filterActive)),
                    ("Search", string.IsNullOrWhiteSpace(searchTerm) ? "(All)" : searchTerm.Trim())
                },
                Columns =
                {
                    new() { Header = "Code",         Width = 80,      Value = o => o.Code ?? "" },
                    new() { Header = "Description",  Relative = 3f,   Value = o => o.Description ?? "" },
                    new() { Header = "Entered By",   Width = 100,     Value = o => o.EnteredByUser ?? "" },
                    new() { Header = "Entered Date", Width = 80,      Value = o => o.EnteredDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Active",       Width = 50,      Value = o => o.Active == true ? "Yes" : "No" },
                }
            };
 
            return File(_pdfReport.Build(def), "application/pdf",
                $"RoadsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
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