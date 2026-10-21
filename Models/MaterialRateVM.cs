namespace CostAccounting.Models
{
    public class MaterialRateVM
    {
        public int ObjectID { get; set; }
        public int MaterialObjectId { get; set; }
        public string? MaterialCode { get; set; }
        public string? MaterialDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
    }

    public class MaterialRateIndexVM
    {
        public IEnumerable<MaterialRateVM> Rates { get; set; } = [];
        public IEnumerable<Material> Materials { get; set; } = [];

        // ── Filters ──────────────────────────────────────────────────────
        public int? FilterMaterialId { get; set; }
        public bool ShowAllMaterials { get; set; } = true;
        public string? StartDateOperator { get; set; } = "<=";
        public DateTime? StartDateFilter { get; set; }
        public string? EndDateOperator { get; set; } = ">=";
        public DateTime? EndDateFilter { get; set; }
        public bool ShowAllDates { get; set; } = true;
        public string? SearchTerm { get; set; }

        // ── Sort ─────────────────────────────────────────────────────────
        public string SortColumn { get; set; } = "MaterialCode";
        public string SortDir { get; set; } = "asc";

        // ── Pagination ───────────────────────────────────────────────────
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}