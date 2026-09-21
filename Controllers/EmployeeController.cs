using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Services.EmployeeService;
using CostAccounting.Models;


namespace CostAccounting.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
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
