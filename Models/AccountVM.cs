using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class AccountVM
    {
        public int ObjectID { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? AccountTypeName { get; set; }
        public int AccountTypeId { get; set; }
        public bool? Active { get; set; }
        public DateTime? EnteredDate { get; set; }
        public string? EnteredByUser { get; set; }
    }

    // ── Create (modal form) ────────────────────────────────────────────────
    public class AccountCreateViewModel
    {
        [Required(ErrorMessage = "Code is required.")]
        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a type.")]
        public int AccountTypeObjectID { get; set; }    // will store 88/89/90

        public int AgencyTypeObjectID { get; set; }
        public bool Active { get; set; } = true;

        [StringLength(50)]
        public string? EnteredByUser { get; set; }
    }

    // ── Inline edit (AJAX) ─────────────────────────────────────────────────
    public class AccountInlineEditViewModel
    {
        [Required]
        public int ObjectID { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a type.")]
        public int AccountTypeObjectID { get; set; }    // 88/89/90

        public int AgencyTypeObjectID { get; set; }
        public bool Active { get; set; }

        [StringLength(50)]
        public string? EnteredByUser { get; set; }
    }

    // ── Index page wrapper ─────────────────────────────────────────────────
    public class AccountIndexVM
    {
        // Table data
        public IEnumerable<AccountVM> Accounts { get; set; } = [];
        public IEnumerable<Lookup> AccountTypes { get; set; } = [];

        // New-record form (modal)
        public AccountCreateViewModel NewAccount { get; set; } = new();

        // Filters
        public string? SearchTerm { get; set; }
        public bool? FilterActive { get; set; }
        public int? FilterTypeId { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string SortColumn { get; set; } = "Code";
        public string SortDirection { get; set; } = "asc";

        // Stats (derived)
        public int ActiveCount => Accounts.Count(a => a.Active == true);
        public int InactiveCount => Accounts.Count(a => a.Active != true);
    }
}
