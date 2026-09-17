using System.Security.Claims;
using CostAccounting.Models;
using CostAccounting.Services.UserService;
using CostAccounting.Services.PasswordHasher;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    [AllowAnonymous]
    public class User_AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _hasher;

        public User_AccountController(IUserService userService, IPasswordHasher hasher)
        {
            _userService = userService;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string returnUrl = null)
        {
            ModelState.Clear();
            if (!TryValidateModel(vm))
                return View(vm);

            var user = await _userService.GetByUsernameAsync(vm.Username);

            // Generic error whether username or password is wrong -
            // don't reveal which one failed to an attacker.
            if (user == null || !_hasher.VerifyPassword(vm.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ObjectID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            await _userService.UpdateLastLoginAsync(user.ObjectID);

            if (user.MustChangePassword)
                return RedirectToAction("ChangePassword", "Profile");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
