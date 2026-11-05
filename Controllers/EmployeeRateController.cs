using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Services;
using CostAccounting.Models;
using CostAccounting.Services.EmployeeRateService;
using CostAccounting.Services.EmployeeService;
using CostAccounting.Services.Reports;
using CostAccounting.Helpers;

namespace CostAccounting.Controllers
{
    public class EmployeeRateController : Controller
    {
        private readonly IEmployeeRateService _employeeRateService;
        private readonly IEmployeeService _employeeService;
        private readonly IPdfReportService _pdfReport;

        public EmployeeRateController(
            IEmployeeRateService employeeRateService,
            IEmployeeService employeeService,
            IPdfReportService pdfReport)
        {
            _employeeRateService = employeeRateService;
            _employeeService = employeeService;
            _pdfReport = pdfReport;
        }

        public async Task<IActionResult> Index(int? employeeObjectId)
        {
            var vm = await _employeeRateService.GetIndexAsync(new EmployeeRateIndexVM
            {
                EmployeeObjectId = employeeObjectId,
                // Default view: each employee's current rate only. The Status dropdown on the
                // page switches to Scheduled, Ended or All.
                Status = "current",
                SortField = "EmployeeName",
                SortDirection = "asc"
            });

            vm.EmployeeOptions = await _employeeService.GetActiveEmployeeOptionsAsync();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetTable([FromQuery] EmployeeRateIndexVM filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            filter.PageSize = PagingDefaults.Normalize(filter.PageSize);

            var vm = await _employeeRateService.GetIndexAsync(filter);
            return PartialView("_EmployeeRateTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vm = await _employeeRateService.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return Json(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeRateVM vm)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.CreateAsync(vm, User.Identity.Name);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] EmployeeRateVM vm)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.UpdateAsync(vm, User.Identity.Name);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkEndDate([FromForm] BulkEndDateRequest request)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.BulkEndDateAsync(request.ObjectIDs, request.EndDate);
            return Json(new { success, message });
        }

        // Returns the computed seasonal periods so the Apply Adjustment modal can pre-fill the
        // Start/End dates without hardcoding the date math in JS:
        //   increase: first Sunday after Thanksgiving -> day before the last Saturday in March
        //   decrease: last Saturday in March -> day before the first Sunday after Thanksgiving
        // nextIsIncrease says which of the two comes up next, so the modal can default to it.
        [HttpGet]
        public IActionResult GetSeasonalDates()
        {
            var today = System.DateTime.Today;
            var (_, _, nextIsIncrease, _) = SeasonalRateHelper.GetUpcoming(today);
            var inc = SeasonalRateHelper.GetIncreasePeriod(today);
            var dec = SeasonalRateHelper.GetDecreasePeriod(today);
            return Json(new
            {
                nextIsIncrease,
                increaseStart = inc.Start.ToString("yyyy-MM-dd"),
                increaseEnd = inc.End.ToString("yyyy-MM-dd"),
                decreaseStart = dec.Start.ToString("yyyy-MM-dd"),
                decreaseEnd = dec.End.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySeasonalAdjustment([FromForm] SeasonalAdjustmentRequest request)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.ApplySeasonalAdjustmentAsync(
                request.IsSeasonal, request.IsIncrease, request.StartDate, request.EndDate,
                request.EmployeeObjectIds, request.SelectedOnly, request.Percent, User.Identity.Name);
            return Json(new { success, message });
        }

        // Export PDF: same filters/sort as the grid, but no paging. If rows are checked in
        // the grid only those are exported, otherwise everything matching the filters.
        [HttpGet]
        public async Task<IActionResult> ExportPdf([FromQuery] EmployeeRateIndexVM filter, [FromQuery] List<int> selectedIds)
        {
            filter.Page = 1;
            filter.PageSize = int.MaxValue;
            var vm = await _employeeRateService.GetIndexAsync(filter);

            var rows = vm.EmployeeRates;
            var hasSelection = selectedIds != null && selectedIds.Any();
            if (hasSelection)
            {
                var set = new HashSet<int>(selectedIds);
                rows = rows.Where(r => set.Contains(r.ObjectID)).ToList();
            }

            var def = new ReportDefinition<EmployeeRateVM>
            {
                Title = "Employee Rate Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Rates",
                Rows = rows,
                Parameters =
                {
                    ("Rates", hasSelection ? "Selected rows only" : ((filter.Status ?? "current").ToLowerInvariant() switch
                    {
                        "all" => "All rates",
                        "scheduled" => "Scheduled rates only",
                        "ended" => "Ended rates only",
                        _ => "Current rates only"
                    })),
                    ("Search", string.IsNullOrWhiteSpace(filter.SearchTerm) ? "(All)" : filter.SearchTerm.Trim()),
                    ("Start Date From", filter.DateRangeStart?.ToString("MM/dd/yyyy") ?? "(Any)"),
                    ("Start Date To", filter.DateRangeEnd?.ToString("MM/dd/yyyy") ?? "(Any)")
                },
                Columns =
                {
                    new() { Header = "Employee",   Relative = 3f, Value = r => $"{r.EmployeeName} ({r.EmployeeCode})" },
                    new() { Header = "Start Date", Width = 70,    Value = r => r.StartDate.ToString("MM/dd/yyyy") },
                    new() { Header = "End Date",   Width = 70,    Value = r => r.EndDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Rate",       Width = 65,    AlignRight = true, Value = r => r.Rate.ToString("C") },
                    new() { Header = "Base Rate",  Width = 65,    AlignRight = true, Value = r => r.BaseRate?.ToString("C") ?? "" },
                    new() { Header = "Status",     Width = 55,    Value = r => r.IsCurrent ? "Current" : r.IsScheduled ? "Scheduled" : "Ended" }
                }
            };

            return File(_pdfReport.Build(def), "application/pdf",
                $"EmployeeRateReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

    }
}
