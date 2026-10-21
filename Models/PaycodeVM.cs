using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{

    public class PaycodeVM
    {
        [Range(1, short.MaxValue, ErrorMessage = "Code must be a positive number.")]
        public short Code { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Abbrev { get; set; }

        public bool Active { get; set; } = true;
        public bool OutOfClass { get; set; }
        public bool ShiftDiff { get; set; }
        public bool OPP { get; set; }

        [Range(0, 9999.9999)]
        public decimal Calculation { get; set; }
    }

    public class PaycodeIndexVM
{
    public List<PaycodeVM> Paycodes { get; set; } = new();

    public string? SearchTerm { get; set; }
    public bool? FilterActive { get; set; } = true;     // default: Active = Yes
    public string SortColumn { get; set; } = "Code";
    public string SortDirection { get; set; } = "asc";

    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
}
