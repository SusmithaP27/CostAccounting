namespace CostAccounting.Models.ViewModels
{
    public class AllowanceVM
    {
        public int ObjectID { get; set; }
        public int? AllowanceTypeObjectID { get; set; }
        public short? Year { get; set; }
        public decimal? Rate { get; set; }
        public int? AllowanceSubTypeObjectID { get; set; }
        public string? SubTypeName { get; set; }     // INTERNAL / EXTERNAL
        public DateTime? EnteredDate { get; set; }
        public string? EnteredByUser { get; set; }
        public bool? Active { get; set; }
    }

    public class AllowanceIndexVM
    {
        public List<AllowanceVM> Allowances { get; set; } = new();
        public List<Lookup> SubTypes { get; set; } = new();   // for filter + modal dropdowns

        public int? FilterYear { get; set; }
        public bool? FilterActive { get; set; }
        public int? FilterSubTypeId { get; set; }

        public string SortColumn { get; set; } = "Year";
        public string SortDirection { get; set; } = "desc";

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}