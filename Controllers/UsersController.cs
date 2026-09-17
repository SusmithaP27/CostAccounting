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
            var users = await _userService.GetAllAsync();
            return View(users);
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
        public async Task<IActionResult> ResetPassword(int targetUserObjectID, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return Json(new { success = false, message = "Password must be at least 8 characters." });

            var (success, message) = await _userService.AdminResetPasswordAsync(targetUserObjectID, newPassword);
            return Json(new { success, message });
        }
    }
}
