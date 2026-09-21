using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Services;
using CostAccounting.Models;
using CostAccounting.Services.EmployeeRateService;
using CostAccounting.Services.EmployeeService;

namespace CostAccounting.Controllers
{
    public class EmployeeRateController : Controller
    {
        private readonly IEmployeeRateService _employeeRateService;
        private readonly IEmployeeService _employeeService;

        public EmployeeRateController(IEmployeeRateService employeeRateService, IEmployeeService employeeService)
        {
            _employeeRateService = employeeRateService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(int? employeeObjectId)
        {
            var vm = await _employeeRateService.GetIndexAsync(new EmployeeRateIndexVM
            {
                EmployeeObjectId = employeeObjectId,
                // Default view: each employee's current rate only. The "Show All Rates"
                // checkbox on the page expands this to every rate, current and ended, for
                // every employee.
                ShowAll = false,
                SortField = "EmployeeName",
                SortDirection = "asc"
            });

            vm.EmployeeOptions = await _employeeService.GetActiveEmployeeOptionsAsync();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetTable([FromQuery] EmployeeRateIndexVM filter)
        {
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

        // Returns the computed seasonal dates (both this year's/next occurrence of each, plus
        // which one is coming up next) so the Apply Seasonal Adjustment modal can pre-fill
        // without hardcoding the date math in JS.
        [HttpGet]
        public IActionResult GetSeasonalDates()
        {
            var today = System.DateTime.Today;
            var (increaseDate, decreaseDate, nextIsIncrease, nextDate) = SeasonalRateHelper.GetUpcoming(today);
            return Json(new
            {
                increaseDate = increaseDate.ToString("yyyy-MM-dd"),
                decreaseDate = decreaseDate.ToString("yyyy-MM-dd"),
                nextIsIncrease,
                nextDate = nextDate.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySeasonalAdjustment([FromForm] SeasonalAdjustmentRequest request)
        {
            ModelState.Clear();
            var (success, message) = await _employeeRateService.ApplySeasonalAdjustmentAsync(
                request.IsIncrease, request.EffectiveDate, request.EmployeeObjectIds, User.Identity.Name);
            return Json(new { success, message });
        }
    }
}
