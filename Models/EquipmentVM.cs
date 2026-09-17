//using System.ComponentModel.DataAnnotations;

//namespace CostAccounting.Models
//{
//    public class EquipmentVM
//    {
//        public int ObjectID { get; set; }
//        public string? Code { get; set; }
//        public string? Category { get; set; }
//        public string? MfgYear { get; set; }
//        public string? DateInService { get; set; }   // ← string, not DateTime?
//        public string? Make { get; set; }
//        public string? Model { get; set; }
//        public string? Department { get; set; }
//        public decimal? CurrentRate { get; set; }
//        public bool? Active { get; set; }
//    }

//    public class EquipmentIndexVM
//    {
//        public IEnumerable<EquipmentVM> Equipments { get; set; } = [];
//        public string? SearchTerm { get; set; }
//        public bool? FilterActive { get; set; }
//        public int? FilterTypeId { get; set; }
//        public int CurrentPage { get; set; } = 1;
//        public int PageSize { get; set; } = 15;
//        public int TotalCount { get; set; }
//        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
//        public int ActiveCount => Equipments.Count(a => a.Active == true);
//        public int InactiveCount => Equipments.Count(a => a.Active != true);
//    }
//}
namespace CostAccounting.Models
{
    public class EquipmentVM
    {
        public int ObjectID { get; set; }
        public string? Code { get; set; }
        public string? Category { get; set; }
        public string? MfgYear { get; set; }
        public DateTime? DateInService { get; set; }
        public string? Make { get; set; }
        public string? EquipmentModel { get; set; }
        public string? Department { get; set; }
        public decimal? CurrentRate { get; set; }
        public bool? Active { get; set; }
    }

    public class EquipmentIndexVM
    {
        public IEnumerable<EquipmentVM> Equipments { get; set; } = [];
        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public int? FilterTypeId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public int ActiveCount => Equipments.Count(a => a.Active == true);
        public int InactiveCount => Equipments.Count(a => a.Active != true);
    }
}