using CostAccounting.Helpers;
using CostAccounting.Models;
using CostAccounting.Services.MaterialService;
using CostAccounting.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class MaterialController : Controller
    {
        private readonly IMaterialService _service;
        private readonly IPdfReportService _pdfReport;

        public MaterialController(IMaterialService service, IPdfReportService pdfReport)
        {
            _service = service;
            _pdfReport = pdfReport;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            bool? filterActive,
            int? filterTypeId,
            string? sortColumn,
            string? sortDir,
            int page = 1,
            int pageSize = 15)
        {
            pageSize = PagingDefaults.Normalize(pageSize);
            bool isFirstVisit = !Request.Query.ContainsKey("filterActive")
                              && !Request.Query.ContainsKey("searchTerm")
                              && !Request.Query.ContainsKey("filterTypeId")
                              && !Request.Query.ContainsKey("sortColumn");

            if (isFirstVisit)
                filterActive = true;

            var (materials, totalCount) = await _service.GetMaterialsAsync(
                searchTerm, filterActive, filterTypeId,
                sortColumn, sortDir,
                page, pageSize);

            var unitTypes = await _service.GetUnitTypesAsync();

            var vm = new MaterialIndexVM
            {
                Materials = materials,
                UnitTypes = unitTypes,
                SearchTerm = searchTerm,
                FilterActive = filterActive,
                FilterTypeId = filterTypeId,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            ViewBag.SortColumn = sortColumn ?? "Code";
            ViewBag.SortDir = sortDir ?? "asc";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_MaterialsTable", vm);

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
            
            var (materials, totalCount) = await _service.GetMaterialsAsync(
                searchTerm, filterActive, filterTypeId,
                sortColumn, sortDirection,
                1, int.MaxValue);

            var vm = new MaterialIndexVM
                {
                    Materials = materials,                
                    SearchTerm = searchTerm,
                    FilterActive = filterActive,
                    FilterTypeId = filterTypeId,
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                    TotalCount = totalCount
            }   ;
            string YesNoAll(bool? v) => v == null ? "All" : (v == true ? "Yes" : "No");
 
            var def = new ReportDefinition<MaterialVM>
            {
                Title = "Materials Report",
                RunBy = User.Identity?.Name ?? "",
                TotalLabel = "Total Materials",
                Rows = (IReadOnlyList<MaterialVM>)vm.Materials,
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
                    new() { Header = "Eff. Date",   Width = 80,      Value = o => o.EffectiveDate?.ToString("MM/dd/yyyy") ?? "" },
                    new() { Header = "Vendor", Width = 80,      Value = o => o.VendorName ?? ""},
                    new() { Header = "Contract",       Width = 50,      Value = o => o.Contract  ?? "" },
                    new() { Header = "Type",         Width = 70,      Value = o => o.Unit ?? ""  },
                    new() { Header = "Rate",         Width = 70,      Value = o => o.Rate.ToString() ?? ""  },
                    new() { Header = "Active",       Width = 50,      Value = o => o.Active == true ? "Yes" : "No" }
                }
            };
 
            return File(_pdfReport.Build(def), "application/pdf",
                $"MaterialsReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var created = await _service.CreateAsync(model);
            return Json(new { success = true, message = "Material created.", data = created });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MaterialVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var updated = await _service.UpdateAsync(model);
            if (updated == null)
                return Json(new { success = false, message = "Material not found." });

            return Json(new { success = true, message = "Saved successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.SoftDeleteAsync(id);
            if (!ok)
                return Json(new { success = false, message = "Material not found." });

            return Json(new { success = true, message = "Material deleted." });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitTypes()
        {
            var types = await _service.GetUnitTypesAsync();
            return Ok(types.Select(t => new { t.ObjectID, t.Code }));
        }

    }
}