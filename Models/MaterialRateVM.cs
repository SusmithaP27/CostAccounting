namespace CostAccounting.Models
{
    public class MaterialRateVM
    {
        public int ObjectID { get; set; }
        public int MaterialObjectId { get; set; }   // MaterialRate.ObjectID (also FK to Material)
        public string? MaterialCode { get; set; }
        public string? MaterialDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
        public decimal BaseRate { get; set; }
        public bool Selected { get; set; }   // checkbox for bulk update
    }

    public class MaterialRateIndexVM
    {
        // Table data
        public IEnumerable<MaterialRateVM> Rates { get; set; } = [];

        // Material picker dropdown
        public IEnumerable<Material> Materials { get; set; } = [];

        // Filters
        public int? FilterMaterialId { get; set; }
        public bool ShowAllMaterials { get; set; } = false;
        public bool ShowAllDates { get; set; } = false;
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }
        public string? SearchTerm { get; set; }

        // Bulk-update fields
        public DateTime? BulkNewEndDate { get; set; }

        // Sort
        public string SortColumn { get; set; } = "MaterialCode";
        public string SortDir { get; set; } = "asc";

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Stats
        public int DisplayedCount => Rates.Count();
    }
}