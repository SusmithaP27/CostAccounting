using CostAccounting.Models;
using CostAccounting.Services.EquipmentRateService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class EquipmentRateController : Controller
    {
        private readonly IEquipmentRateService _service;

        public EquipmentRateController(IEquipmentRateService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            int? filterEquipmentId,
            bool showAllEquipment = true,
            string? startDateOperator = "<=",
            DateTime? startDateFilter = null,
            string? endDateOperator = ">=",
            DateTime? endDateFilter = null,
            bool showAllDates = true,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 25)
        {
            var equipmentList = await _service.GetEquipmentListAsync();

            // FIX: No more "default to today" fallback. Dates start out fully
            // unbounded (showAllDates = true, both filter values null) so the
            // grid shows every rate by default. Date filtering only kicks in
            // once the user unchecks "Show all dates" and actually picks a
            // Start Date and/or End Date — each side applies independently,
            // so picking just one leaves the other side open-ended.
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var (rates, totalCount) = await _service.GetRatesAsync(
                filterEquipmentId, showAllEquipment,
                startDateOperator, startDateFilter,
                endDateOperator, endDateFilter,
                showAllDates, searchTerm,
                page, pageSize);

            var vm = new EquipmentRateIndexVM
            {
                Rates = rates,
                EquipmentList = equipmentList,
                FilterEquipmentId = filterEquipmentId,
                ShowAllEquipment = showAllEquipment,
                StartDateOperator = startDateOperator,
                StartDateFilter = startDateFilter,
                EndDateOperator = endDateOperator,
                EndDateFilter = endDateFilter,
                ShowAllDates = showAllDates,
                SearchTerm = searchTerm,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            if (isAjax)
                return PartialView("_EquipmentRatesTable", vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EquipmentRateVM model)
        {
            ModelState.Clear();

            if (model.EquipmentObjectId == 0)
                return Json(new { success = false, message = "Equipment is required." });

            var created = await _service.CreateAsync(model);
            return Json(new { success = true, message = "Rate added.", data = created });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(EquipmentRateVM model)
        {
            ModelState.Clear();

            if (model.ObjectID == 0)
                return Json(new { success = false, message = "Invalid rate ID." });

            var updated = await _service.UpdateAsync(model);
            if (updated == null)
                return Json(new { success = false, message = "Rate not found." });

            return Json(new { success = true, message = "Saved successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateEndDate(List<int> objectIds, DateTime newEndDate)
        {
            if (objectIds == null || objectIds.Count == 0)
                return Json(new { success = false, message = "No rows selected." });

            var count = await _service.BulkUpdateEndDateAsync(objectIds, newEndDate);
            return Json(new { success = true, message = $"{count} row(s) updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> objectIds)
        {
            if (objectIds == null || objectIds.Count == 0)
                return Json(new { success = false, message = "No rows selected." });

            var count = await _service.BulkDeleteAsync(objectIds);
            return Json(new { success = true, message = $"{count} row(s) deleted." });
        }
    }
}

