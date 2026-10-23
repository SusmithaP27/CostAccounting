using CostAccounting.Models;
using CostAccounting.Services.AccountService;
using Microsoft.AspNetCore.Mvc;
using CostAccounting.Helpers;
using CostAccounting.Services.Reports;

namespace CostAccounting.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IPdfReportService _pdfReport;
 

        public AccountController(IAccountService accountService, IPdfReportService pdfReport)
        {
            _accountService = accountService;
            _pdfReport = pdfReport;
        }


        // ── GET /Account/Index ─────────────────────────────────────────────
        public async Task<IActionResult> Index(
    string? searchTerm,
    bool? filterActive = true,
    int? filterTypeId = null,
    int page = 1,
    int pageSize = 15,
    string sortColumn = "Code",
    string sortDirection = "asc")
{
    
    if (page < 1) page = 1;
    pageSize = PagingDefaults.Normalize(pageSize);

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
        // Same filters and sort as the grid, but no paging (page 1, every row).
        // Do NOT run PagingDefaults.Normalize on the size here: int.MaxValue is intentional.
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string sortColumn = "Code",
            string sortDirection = "asc")
        {
            // var vm = await _accountService.GetAccountsAsync(
            //         searchTerm, filterActive, filterTypeId, 1, int.MaxValue, sortColumn, sortDirection);
            var (items, total) = await _accountService.GetAccountsAsync(
        searchTerm, filterActive, filterTypeId, 1, int.MaxValue, sortColumn, sortDirection);

    var vm = new AccountIndexVM
    {
        Accounts = items,
        SearchTerm = searchTerm,
        FilterActive = filterActive,
        FilterTypeId = filterTypeId,
        CurrentPage = 1,
        PageSize = int.MaxValue,
        TotalCount = total,
        SortColumn = sortColumn,
        SortDirection = sortDirection
    };
 
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
 
            var def = new ReportDefinition<AccountVM>
            {
                Title = "Accounts Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Accounts",
                Rows = (IReadOnlyList<AccountVM>)vm.Accounts,
                Parameters =
                {
                    ("Active", YesNoAll(filterActive)),
                    ("Id", filterTypeId?.ToString() ?? ""),
                    ("Search", string.IsNullOrWhiteSpace(searchTerm) ? "(All)" : searchTerm.Trim())
                },
                Columns =
                {
                    new() { Header = "Code",         Width = 110,      Value = o => o.Code ?? "" },
                    new() { Header = "Description",  Relative = 2f,   Value = o => o.Description ?? "" },
                    new() { Header = "Entered By",   Width = 100,     Value = o => o.EnteredByUser ?? "" },
                    new() { Header = "Entered Date", Width = 80,      Value = o => o.EnteredDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Active",       Width = 50,      Value = o => o.Active == true ? "Yes" : "No" },
                    new() { Header = "Type",         Width = 70,      Value = o => o.AccountTypeName ?? ""  }
                }
            };
 
            return File(_pdfReport.Build(def), "application/pdf",
                $"AccountsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
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
