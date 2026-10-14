// Controllers/PaycodesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CostAccounting.Services.PaycodeService;
using CostAccounting.Models;

[Authorize]
public class PaycodeController : Controller
{
    private readonly IPaycodeService _service;
    public PaycodeController(IPaycodeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Index(PaycodeIndexVM q)
    {
        ModelState.Clear();
        return View(await _service.GetPagedAsync(q));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaycodeVM vm)
    {
        ModelState.Clear();

        if (vm.Code <= 0)
            return Json(new { success = false, message = "Code must be a positive number." });

        if (vm.Calculation < 0 || vm.Calculation >= 10000)
            return Json(new { success = false, message = "Calculation must be between 0 and 9999.9999." });

        var (ok, message) = await _service.CreateAsync(vm, User.Identity?.Name ?? "unknown");
        return Json(new { success = ok, message });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(PaycodeVM vm)
    {
        ModelState.Clear();

        if (vm.Calculation < 0 || vm.Calculation >= 10000)
            return Json(new { success = false, message = "Calculation must be between 0 and 9999.9999." });

        var (ok, message) = await _service.UpdateAsync(vm);
        return Json(new { success = ok, message });
    }
}