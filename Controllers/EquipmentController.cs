//using CostAccounting.Models;
//using CostAccounting.Services.EquipmentService;
//using Microsoft.AspNetCore.Mvc;

//namespace CostAccounting.Controllers
//{
//    public class EquipmentController : Controller
//    {
//        private readonly IEquipmentService _service;

//        public EquipmentController(IEquipmentService service)
//        {
//            _service = service;
//        }

//        public async Task<IActionResult> Index(
//            string? searchTerm,
//            bool? filterActive,
//            int? filterTypeId,
//            string? sortColumn,
//            string? sortDir,
//            int page = 1,
//            int pageSize = 15)
//        {
//            var (equipments, totalCount) = await _service.GetEquipmentsAsync(
//                searchTerm, filterActive, filterTypeId,
//                sortColumn, sortDir, page, pageSize);

//            var vm = new EquipmentIndexVM
//            {
//                Equipments = equipments,
//                SearchTerm = searchTerm,
//                FilterActive = filterActive,
//                FilterTypeId = filterTypeId,
//                CurrentPage = page,
//                PageSize = pageSize,
//                TotalCount = totalCount
//            };

//            ViewBag.SortColumn = sortColumn ?? "Code";
//            ViewBag.SortDir = sortDir ?? "asc";

//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                return PartialView("_EquipmentsTable", vm);

//            return View(vm);
//        }

//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> Create(EquipmentVM model)
//        //{
//        //    if (string.IsNullOrWhiteSpace(model.Code))
//        //        return Json(new { success = false, message = "Code is required." });

//        //    var created = await _service.CreateAsync(model);
//        //    return Json(new { success = true, message = "Equipment created.", data = created });
//        //}


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(EquipmentVM model)
//        {
//            // Read directly from form as fallback if model binding missed it
//            if (string.IsNullOrWhiteSpace(model.Code))
//                model.Code = Request.Form["Code"].FirstOrDefault()?.Trim();

//            if (string.IsNullOrWhiteSpace(model.Code))
//                return Json(new { success = false, message = "Code is required." });

//            // Manually hydrate other fields if model binding is still off
//            if (string.IsNullOrWhiteSpace(model.Category)) model.Category = Request.Form["Category"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(model.MfgYear)) model.MfgYear = Request.Form["MfgYear"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(model.DateInService)) model.DateInService = Request.Form["DateInService"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(model.Make)) model.Make = Request.Form["Make"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(model.Model)) model.Model = Request.Form["Model"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(model.Department)) model.Department = Request.Form["Department"].FirstOrDefault();

//            // Parse Active from form if model binding missed it
//            if (model.Active == null)
//            {
//                var activeStr = Request.Form["Active"].FirstOrDefault();
//                model.Active = activeStr?.ToLower() == "true";
//            }

//            var created = await _service.CreateAsync(model);
//            return Json(new { success = true, message = "Equipment created.", data = created });
//        }

//        //[HttpPost]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> Update(EquipmentVM model)
//        //{
//        //    ModelState.Clear();
//        //    if (model.ObjectID == 0)
//        //        return Json(new { success = false, message = "Invalid equipment ID." });
//        //    var updated = await _service.UpdateAsync(model);
//        //    if (updated == null)
//        //        return Json(new { success = false, message = "Equipment not found." });

//        //    return Json(new { success = true, message = "Saved successfully." });
//        //}

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Update()
//        {
//            var objectID = 0;
//            int.TryParse(Request.Form["ObjectID"].FirstOrDefault(), out objectID);

//            if (objectID == 0)
//                return Json(new { success = false, message = "Invalid equipment ID." });

//            var model = new EquipmentVM
//            {
//                ObjectID = objectID,
//                Category = Request.Form["Category"].FirstOrDefault(),
//                MfgYear = Request.Form["MfgYear"].FirstOrDefault(),
//                DateInService = Request.Form["DateInService"].FirstOrDefault(),
//                Make = Request.Form["Make"].FirstOrDefault(),
//                Model = Request.Form["Model"].FirstOrDefault(),
//                Department = Request.Form["Department"].FirstOrDefault(),
//                Active = Request.Form["Active"].FirstOrDefault()?.ToLower() == "true"
//            };

//            var updated = await _service.UpdateAsync(model);
//            if (updated == null)
//                return Json(new { success = false, message = "Equipment not found." });

//            return Json(new { success = true, message = "Saved successfully." });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int objectID)
//        {
//            var entity = await _service.GetByIdAsync(objectID);
//            if (entity == null)
//                return Json(new { success = false, message = "Equipment not found." });

//            await _service.DeleteAsync(objectID);
//            return Json(new { success = true, message = "Equipment deleted." });
//        }
//    }
//}
using CostAccounting.Models;
using CostAccounting.Services.EquipmentService;
using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly IEquipmentService _service;

        public EquipmentController(IEquipmentService service)
        {
            _service = service;
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
            var (equipments, totalCount) = await _service.GetEquipmentsAsync(
                searchTerm, filterActive, filterTypeId,
                sortColumn, sortDir, page, pageSize);

            var vm = new EquipmentIndexVM
            {
                Equipments = equipments,
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
                return PartialView("_EquipmentsTable", vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EquipmentVM model)
        {
            // Log raw form data
            Console.WriteLine("----------------------------------------");
            foreach (var key in Request.Form.Keys)
                Console.WriteLine($"FORM KEY: {key} = {Request.Form[key]}");

            // Log model state
            Console.WriteLine($"model.Code = '{model.Code}'");
            Console.WriteLine($"model.ObjectID = '{model.ObjectID}'");
            Console.WriteLine($"ModelState.IsValid = {ModelState.IsValid}");
            Console.WriteLine("----------------------------------------");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }
            Console.WriteLine("**************************************************");
            Console.WriteLine(model.Code);
            Console.WriteLine(model);
            Console.WriteLine("**************************************************");
            var created = await _service.CreateAsync(model);
            return Json(new { success = true, message = "Equipment created.", data = created });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(EquipmentVM model)
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
                return Json(new { success = false, message = "Equipment not found." });

            return Json(new { success = true, message = "Saved successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var ok = await _service.SoftDeleteAsync(id);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}