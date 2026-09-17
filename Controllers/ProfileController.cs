using System.Security.Claims;
using CostAccounting.Models;
using CostAccounting.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetByIdAsync(CurrentUserId);
            if (user == null) return NotFound();

            var vm = new ProfileVM
            {
                ObjectID = user.ObjectID,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                LastLoginDate = user.LastLoginDate
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ResetPasswordVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordVM vm)
        {
            ModelState.Clear();
            if (!TryValidateModel(vm))
                return View(vm);

            var (success, message) = await _userService.SelfResetPasswordAsync(
                CurrentUserId, vm.CurrentPassword, vm.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(vm);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index");
        }
    }
}