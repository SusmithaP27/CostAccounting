namespace CostAccounting.Models
{
    public class EquipmentRateVM
    {
        public int ObjectID { get; set; }
        public int EquipmentObjectId { get; set; }

        public string? EquipmentCode { get; set; }
        public string? EquipmentMake { get; set; }
        public string? EquipmentModel { get; set; }
        public string EquipmentDisplay =>
            $"{EquipmentCode} ({EquipmentMake} {EquipmentModel})".Trim();

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
        public decimal MiscRate { get; set; }
    }

    public class EquipmentLookupVM
    {
        public int ObjectID { get; set; }
        public string? Code { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string Display => $"{Code} ({Make} {Model})".Trim();
    }

    public class EquipmentRateIndexVM
    {
        public IEnumerable<EquipmentRateVM> Rates { get; set; } = [];
        public IEnumerable<EquipmentLookupVM> EquipmentList { get; set; } = [];

        // ── Filters ──────────────────────────────────────────────────────
        public int? FilterEquipmentId { get; set; }
        public bool ShowAllEquipment { get; set; } = true;
        public string? StartDateOperator { get; set; } = "<=";
        public DateTime? StartDateFilter { get; set; }
        public string? EndDateOperator { get; set; } = ">=";
        public DateTime? EndDateFilter { get; set; }
        public bool ShowAllDates { get; set; }
        public string? SearchTerm { get; set; }

        // ── Pagination ───────────────────────────────────────────────────
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
