using CostAccounting.Models;
using CostAccounting.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _userService.GetIndexAsync(new UsersIndexPageVM());
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetTable([FromQuery] UsersIndexPageVM filter)
        {
            var vm = await _userService.GetIndexAsync(filter);
            return PartialView("_UserTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vm = await _userService.GetByIdVMAsync(id);
            if (vm == null) return NotFound();
            return Json(vm);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateUserVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            ModelState.Clear();
            if (!TryValidateModel(vm))
                return View(vm);

            var (success, message) = await _userService.CreateUserAsync(vm, User.Identity.Name);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(vm);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] UserIndexVM model)
        {
            ModelState.Clear();
            var (success, message) = await _userService.UpdateUserAsync(model);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            ModelState.Clear();
            var (success, message) = await _userService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int targetUserObjectID, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return Json(new { success = false, message = "Password must be at least 8 characters." });

            var (success, message) = await _userService.AdminResetPasswordAsync(targetUserObjectID, newPassword);
            return Json(new { success, message });
        }
    }
}
