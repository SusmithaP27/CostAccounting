using CostAccounting.Models;
using CostAccounting.Services.AccountService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
            => _accountService = accountService;



        // ── GET /Account/Index ─────────────────────────────────────────────
        public async Task<IActionResult> Index(
    string? searchTerm,
    bool? filterActive,
    int? filterTypeId,
    int page = 1,
    string sortColumn = "Code",
    string sortDirection = "asc")
{
    const int pageSize = 15;

    // True first visit (no querystring at all) → default to Active only.
    // Once the filter form has been submitted once, filterActive is always
    // present in the query (even as "" for "All"), so this only fires once.
    bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                      && !Request.Query.ContainsKey("searchTerm")
                      && !Request.Query.ContainsKey("filterTypeId")
                      && !Request.Query.ContainsKey("sortColumn");

    if (isFirstVisit)
        filterActive = true;

    var types = await _accountService.GetAccountTypesAsync();
    var (items, total) = await _accountService.GetAccountsAsync(
        searchTerm, filterActive, filterTypeId, page, pageSize, sortColumn, sortDirection);

    var vm = new AccountIndexVM
    {
        Accounts = items,
        AccountTypes = types,
        SearchTerm = searchTerm,
        FilterActive = filterActive,
        FilterTypeId = filterTypeId,
        CurrentPage = page,
        PageSize = pageSize,
        TotalCount = total,
        SortColumn = sortColumn,
        SortDirection = sortDirection
    };

    if (TempData["ShowModal"] is true)
        ViewBag.ShowModal = true;

    return View(vm);
}

        // ── POST /Account/Create ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            
            model.EnteredByUser = User.Identity?.Name ?? "";
            if (!ModelState.IsValid)
            {
                // Return JSON error list so the modal can display them inline
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var (success, message) = await _accountService.CreateAccountAsync(model);
            return Json(new { success, message });
        }

        // ── POST /Account/InlineEdit ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineEdit(AccountInlineEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var (success, message) = await _accountService.UpdateAccountAsync(model);
            return Json(new { success, message });
        }

        // POST: /Account/SoftDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var (success, message) = await _accountService.SoftDeleteAsync(id);
            return Json(new { success, message });
        }

    }
}
