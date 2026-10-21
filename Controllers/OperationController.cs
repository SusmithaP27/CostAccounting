using CostAccounting.Helpers;
using CostAccounting.Models;
using CostAccounting.Services.OperationService;
using CostAccounting.Services.Reports;
using Microsoft.AspNetCore.Mvc;
 
namespace CostAccounting.Controllers
{
    public class OperationController : Controller
    {
        private readonly IOperationService _operationService;
        private readonly IPdfReportService _pdfReport;
 
        public OperationController(IOperationService operationService, IPdfReportService pdfReport)
        {
            _operationService = operationService;
            _pdfReport = pdfReport;
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
            pageSize = PagingDefaults.Normalize(pageSize);
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
 
        // GET: /Operation/ExportPdf
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
            var vm = await _operationService.GetPagedOperationsAsync(
                searchTerm, filterActive, filterEngineering, 1, int.MaxValue, sortColumn, sortDirection);
 
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
 
            var def = new ReportDefinition<Operation>
            {
                Title = "Operations Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Operations",
                Rows = vm.Operations,
                Parameters =
                {
                    ("Active", YesNoAll(filterActive)),
                    ("Engineering", YesNoAll(filterEngineering)),
                    ("Search", string.IsNullOrWhiteSpace(searchTerm) ? "(All)" : searchTerm.Trim())
                },
                Columns =
                {
                    new() { Header = "Code",         Width = 80,      Value = o => o.Code ?? "" },
                    new() { Header = "Description",  Relative = 3f,   Value = o => o.Description ?? "" },
                    new() { Header = "Entered By",   Width = 100,     Value = o => o.EnteredByUser ?? "" },
                    new() { Header = "Entered Date", Width = 80,      Value = o => o.EnteredDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Active",       Width = 50,      Value = o => o.Active == true ? "Yes" : "No" },
                    new() { Header = "Engineering",  Width = 70,      Value = o => o.Engineering == true ? "Yes" : "No" }
                }
            };
 
            return File(_pdfReport.Build(def), "application/pdf",
                $"OperationsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
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