using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Services.EmployeeService;
using CostAccounting.Services.Reports;
using CostAccounting.Models;
using CostAccounting.Helpers;

namespace CostAccounting.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPdfReportService _pdfReport;

        public EmployeeController(IEmployeeService employeeService, IPdfReportService pdfReport)
        {
            _employeeService = employeeService;
            _pdfReport = pdfReport;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _employeeService.GetIndexAsync(new EmployeeIndexVM());
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetTable([FromQuery] EmployeeIndexVM filter)
        {
            var vm = await _employeeService.GetIndexAsync(filter);
            return PartialView("_EmployeeTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vm = await _employeeService.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return Json(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf([FromQuery] EmployeeIndexVM filter)
        {
            // Same filters and sort as the grid, but no paging.
            filter.Page = 1;
            filter.PageSize = int.MaxValue;
            var vm = await _employeeService.GetIndexAsync(filter);

            var def = new ReportDefinition<EmployeeVM>
            {
                Title = "Employee Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Employees",
                Rows = vm.Employees,
                Parameters =
                {
                    ("Status", filter.ShowInactive ? "Active and Inactive" : "Active only"),
                    ("Search", string.IsNullOrWhiteSpace(filter.SearchTerm) ? "(All)" : filter.SearchTerm.Trim())
                },
                Columns =
                {
                    new() { Header = "Code",           Width = 45,      Value = e => e.Code },
                    new() { Header = "Munis Code",     Width = 55,      Value = e => e.MunisCode?.ToString() ?? "" },
                    new() { Header = "Full Name",      Relative = 2.2f, Value = e => e.FullName },
                    new() { Header = "Account",        Relative = 2.5f, Value = e => e.AccountName ?? "" },
                    new() { Header = "Hire Date",      Width = 65,      Value = e => e.HireDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Longevity Date", Width = 70,      Value = e => e.LongevityDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Base Rate",      Width = 60,      AlignRight = true, Value = e => e.BaseRate?.ToString("C") ?? "" },
                    new() { Header = "Current Rate",   Width = 65,      AlignRight = true, Value = e => e.CurrentRate?.ToString("C") ?? "" },
                    new() { Header = "Status",         Width = 50,      Value = e => e.Active ? "Active" : "Inactive" }
                }
            };

            return File(_pdfReport.Build(def), "application/pdf",
                $"EmployeeReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeVM vm)
        {
            ModelState.Clear();
            var (success, message) = await _employeeService.CreateAsync(vm, User.Identity.Name);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] EmployeeVM vm)
        {
            ModelState.Clear();
            var (success, message) = await _employeeService.UpdateAsync(vm, User.Identity.Name);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            ModelState.Clear();
            var (success, message) = await _employeeService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }
    }
}