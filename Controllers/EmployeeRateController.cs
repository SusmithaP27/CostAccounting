using Microsoft.AspNetCore.Mvc;
using CostAccounting.Models;
using CostAccounting.Services.EmployeeRateService;

namespace CostAccounting.Controllers
{
    public class EmployeeRateController : Controller
    {
        private readonly IEmployeeRateService _employeeRateService;

        public EmployeeRateController(IEmployeeRateService employeeRateService)
        {
            _employeeRateService = employeeRateService;
        }

        public async Task<IActionResult> Index(int? employeeObjectId)
        {
            // var vm = await _employeeRateService.GetIndexAsync(new EmployeeRateIndexVM { EmployeeObjectId = employeeObjectId });
            // return View(vm);
            var vm = await _employeeRateService.GetIndexAsync(new EmployeeRateIndexVM
            {
                EmployeeObjectId = employeeObjectId,
                // This is a standalone tab covering every employee's rate history, not just
                // whoever's currently active — always show ended rates alongside current ones.
                ShowInactiveEmployees = false,
                SortField = "StartDate",
                SortDirection = "desc",
                Page = 1,
                PageSize = 15
            });    
             vm.Employees =
        await _employeeRateService.GetActiveEmployeesAsync();           
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

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([FromForm] EmployeeRateVM vm)
        // {
        //     ModelState.Clear();
        //     var (success, message) = await _employeeRateService.CreateAsync(vm, User.Identity.Name);
        //     return Json(new { success, message });
        // }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([FromForm] EmployeeRateVM vm)
{
    try
    {
        ModelState.Clear();

        if (vm.EmployeeObjectId <= 0)
        {
            return Json(new
            {
                success = false,
                message = "Please select an employee."
            });
        }

        if (vm.StartDate == default)
        {
            return Json(new
            {
                success = false,
                message = "Please enter a start date."
            });
        }

        if (vm.Rate < 0)
        {
            return Json(new
            {
                success = false,
                message = "Rate cannot be negative."
            });
        }

        var enteredByUser =
            User?.Identity?.Name ?? "System";

        var (success, message) =
            await _employeeRateService.CreateAsync(
                vm,
                enteredByUser);

        return Json(new
        {
            success,
            message
        });
    }
    catch (Exception ex)
    {
        return Json(new
        {
            success = false,
            message = ex.InnerException?.Message
                      ?? ex.Message
        });
    }
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
    }
}
