namespace CostAccounting.Models
{
    public class MaterialVM
    {
        public int ObjectID { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? VendorName { get; set; }
        public string? Contract { get; set; }
        public int? UnitTypeObjectID { get; set; }
        public decimal Rate { get; set; }
        public string? Unit { get; set; }
        public bool? Active { get; set; }
    }

    public class MaterialIndexVM
    {
        public IEnumerable<MaterialVM> Materials { get; set; } = [];
        public IEnumerable<Lookup> UnitTypes { get; set; } = [];

        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public int? FilterTypeId { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public int ActiveCount => Materials.Count(a => a.Active == true);
        public int InactiveCount => Materials.Count(a => a.Active != true);
    }
}