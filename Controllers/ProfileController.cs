using System.Security.Claims;
using CostAccounting.Models;
using CostAccounting.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    // User_AccountController.Login redirects here (RedirectToAction("ChangePassword",
    // "Profile")) whenever user.MustChangePassword is true — first sign-in or after an
    // admin-initiated password reset from the Users grid.
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
public async Task<IActionResult> Index()
{
    var user = await _userService.GetByIdAsync(CurrentUserId());
    if (user == null) return NotFound();

    // Don't let someone with a temporary password browse around first
    if (user.MustChangePassword)
        return RedirectToAction(nameof(ChangePassword));

    var vm = new ProfileVM
    {
        Username      = user.Username,
        FullName      = user.FullName,
        Email         = user.Email,
        Role          = user.Role,            // adjust to your model, see below
        LastLoginDate = user.LastLoginDate
    };

    return View(vm);
}

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            ViewBag.IsFirstSignIn = await IsMustChangePasswordAsync();
            return View(new ChangePasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsFirstSignIn = await IsMustChangePasswordAsync();
                return View(vm);
            }

            var userId = CurrentUserId();
            var (success, message) = await _userService.SelfResetPasswordAsync(userId, vm.CurrentPassword, vm.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.IsFirstSignIn = await IsMustChangePasswordAsync();
                return View(vm);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index", "Home");
        }

        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // User_AccountController doesn't put MustChangePassword in the auth cookie's claims,
        // so this looks it up from the DB each time rather than reading it off User.Claims —
        // that also means the "Set Your Password" banner correctly disappears the moment the
        // password is actually changed, without needing to sign out/in first.
        private async Task<bool> IsMustChangePasswordAsync()
        {
            var user = await _userService.GetByIdAsync(CurrentUserId());
            return user?.MustChangePassword ?? false;
        }
    }
}
