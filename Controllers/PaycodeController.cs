// Controllers/PaycodesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CostAccounting.Services.PaycodeService;
using CostAccounting.Models;
using CostAccounting.Helpers;
using CostAccounting.Services.Reports;

[Authorize]
public class PaycodeController : Controller
{
    private readonly IPaycodeService _service;
    private readonly IPdfReportService _pdfReport;
    public PaycodeController(IPaycodeService service, IPdfReportService pdfReport)
    {
        _service = service;
        _pdfReport = pdfReport;
    } 

    [HttpGet]
    public async Task<IActionResult> Index(PaycodeIndexVM q)
    {
        ModelState.Clear();
        if (q.CurrentPage < 1) q.CurrentPage = 1;
        q.PageSize = PagingDefaults.Normalize(q.PageSize);
        return View(await _service.GetPagedAsync(q));
    }
        [HttpGet]
        public async Task<IActionResult> ExportPdf([FromQuery] PaycodeIndexVM q)
        {
            // Same filters and sort as the grid, but no paging.
            q.CurrentPage = 1;
            q.PageSize = int.MaxValue;
            var vm = await _service.GetPagedAsync(q);
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
            var def = new ReportDefinition<PaycodeVM>
            {
                Title = "Paycode Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Paycodes",
                Rows = vm.Paycodes,
                Parameters =
                {
                    ("Active", YesNoAll(q.FilterActive)),
                    ("Search", string.IsNullOrWhiteSpace(q.SearchTerm) ? "(All)" : q.SearchTerm.Trim())
                },
                Columns =
                {
                    new() { Header = "Code",           Width = 50,      Value = e => e.Code.ToString() ?? ""},
                    new() { Header = "Description",    Relative = 3f,   Value = o => o.Description ?? "" },
                    new() { Header = "Abbrev",         Width = 80,      Value = e => e.Abbrev?.ToString() ?? "" },
                    new() { Header = "Calculation",    Width = 70,      Value = e => e.Calculation.ToString() ?? ""},
                    new() { Header = "Out of Class",   Width = 50,      Value = e => e.OutOfClass ? "Yes" : "No"},
                    new() { Header = "Shift Diff",     Width = 50,      Value = e => e.ShiftDiff ? "Yes" : "No"},
                    new() { Header = "OPP",            Width = 50,      Value = e => e.OPP ? "Yes" : "No" },
                    new() { Header = "Active",         Width = 50,      Value = e => e.Active ? "Yes" : "No" }
                }
            };

            return File(_pdfReport.Build(def), "application/pdf",
                $"PaycodesReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaycodeVM vm)
    {
        ModelState.Clear();

        if (vm.Code <= 0)
            return Json(new { success = false, message = "Code must be a positive number." });

        if (vm.Calculation < 0 || vm.Calculation >= 10000)
            return Json(new { success = false, message = "Calculation must be between 0 and 9999.9999." });

        var (ok, message) = await _service.CreateAsync(vm, User.Identity?.Name ?? "unknown");
        return Json(new { success = ok, message });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(PaycodeVM vm)
    {
        ModelState.Clear();

        if (vm.Calculation < 0 || vm.Calculation >= 10000)
            return Json(new { success = false, message = "Calculation must be between 0 and 9999.9999." });

        var (ok, message) = await _service.UpdateAsync(vm);
        return Json(new { success = ok, message });
    }
}