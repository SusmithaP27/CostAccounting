using CostAccounting.Services;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Helpers;
using CostAccounting.Services.Reports;
using CostAccounting.Models;
using CostAccounting.Models.ViewModels;

namespace CostAccounting.Controllers
{
    public class AllowanceController : Controller
    {
        private readonly IAllowanceService _allowanceService;
        private readonly IPdfReportService _pdfReport;

        public AllowanceController(IAllowanceService allowanceService, IPdfReportService pdfReport)
        {
            _allowanceService = allowanceService;
            _pdfReport = pdfReport;
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
        // GET: /Allowance/ExportPdf
        // Same filters and sort as the grid, but no paging (page 1, every row).
        // Do NOT run PagingDefaults.Normalize on the size here: int.MaxValue is intentional.
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            int? filterYear,
            bool? filterActive,
            int? filterSubTypeId,
            string sortColumn = "Code",
            string sortDirection = "asc")
        {
            var vm = await _allowanceService.GetAllowancesAsync(
                filterYear, filterActive, filterSubTypeId, sortColumn, sortDirection, 1, int.MaxValue);
 
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
 
            var def = new ReportDefinition<AllowanceVM>
            {
                Title = "Allowance Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Allowances",
                Rows = vm.Allowances,
                Parameters =
                {
                    ("Active", YesNoAll(filterActive)),
                    ("Year", filterYear?.ToString() ?? ""),
                    ("SubTypeId",filterSubTypeId?.ToString() ?? "")
                    
                },
                Columns =
                {
                    new() { Header = "Year",         Width = 80,      Value = o => o.Year?.ToString() ?? "" },
                    new() { Header = "County Benefit",  Relative = 3f,   Value = o => o.Rate?.ToString() ?? "" },
                    new() { Header = "Type",   Width = 100,     Value = o => o.SubTypeName ?? "" }
                }
            };
 
            return File(_pdfReport.Build(def), "application/pdf",
                $"AllowancesReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
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