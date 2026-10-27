using CostAccounting.Helpers;
using CostAccounting.Models;
using CostAccounting.Services.EquipmentRateService;
using CostAccounting.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class EquipmentRateController : Controller
    {
        private readonly IEquipmentRateService _service;
        private readonly IPdfReportService _pdfReport;

        public EquipmentRateController(IEquipmentRateService service, IPdfReportService pdfReport)
        {
            _service = service;
            _pdfReport = pdfReport;
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
            int pageSize = 15)
        {
            pageSize = PagingDefaults.Normalize(pageSize);
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

        [HttpGet]
        public async Task<IActionResult> ExportPdf(
    int? filterEquipmentId,
    bool showAllEquipment = true,
    string? startDateOperator = "<=",
    DateTime? startDateFilter = null,
    string? endDateOperator = ">=",
    DateTime? endDateFilter = null,
    bool showAllDates = true,
    string? searchTerm = null,
    string? selectedIds = null)
        {
            // Same filters as the grid, but no paging.
            var (rates, _) = await _service.GetRatesAsync(
                filterEquipmentId, showAllEquipment,
                startDateOperator, startDateFilter,
                endDateOperator, endDateFilter,
                showAllDates, searchTerm,
                1, int.MaxValue);

            var rows = rates.ToList();

            var ids = (selectedIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var i) ? i : 0)
                .Where(i => i > 0)
                .ToList();

            var hasSelection = ids.Any();
            if (hasSelection)
            {
                var set = new HashSet<int>(ids);
                rows = rows.Where(r => set.Contains(r.ObjectID)).ToList();
            }

            string datesText = "All dates";
            if (!showAllDates)
            {
                var parts = new List<string>();
                if (startDateFilter.HasValue) parts.Add($"Start {startDateOperator} {startDateFilter:MM/dd/yyyy}");
                if (endDateFilter.HasValue) parts.Add($"End {endDateOperator} {endDateFilter:MM/dd/yyyy}");
                datesText = parts.Any() ? string.Join(" and ", parts) : "All dates";
            }

            var def = new ReportDefinition<EquipmentRateVM>
            {
                Title = "Equipment Rate Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Rates",
                Rows = rows,
                Parameters =
        {
            ("Rows", hasSelection ? "Selected rows only" : "All matching rows"),
            ("Equipment", showAllEquipment ? "All equipment" : "Selected equipment"),
            ("Dates", datesText),
            ("Search", string.IsNullOrWhiteSpace(searchTerm) ? "(All)" : searchTerm.Trim())
        },
                Columns =
        {
            new() { Header = "Equipment",   Relative = 3f, Value = r => r.EquipmentDisplay ?? "" },
            //new() { Header = "Equipment",   Relative = 3f, Value = r => r.EquipmentCode ?? "" },
            new() { Header = "Start Date",  Width = 70,    Value = r => r.StartDate?.ToString("MM/dd/yyyy") ?? "" },
            new() { Header = "End Date",    Width = 70,    Value = r => r.EndDate?.ToString("MM/dd/yyyy") ?? "" },
            new() { Header = "Rate",        Width = 70,    AlignRight = true, Value = r => r.Rate.ToString("N4") },
            new() { Header = "Misc.Rate",   Width = 70,    AlignRight = true, Value = r => r.MiscRate.ToString("N4") }
        }
            };

            return File(_pdfReport.Build(def), "application/pdf",
                $"EquipmentRateReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}

