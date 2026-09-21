using System;
using System.Collections.Generic;

namespace CostAccounting.Models
{
    public class EmployeeVM
    {
        public int ObjectID { get; set; }

        // Auto-generated server-side from MunisCode (padded to 4 digits) — never posted from
        // the Add/Edit forms, only ever returned for display.
        public string Code { get; set; }

        public int? MunisCode { get; set; }
        public string FirstName { get; set; }
        public string MI { get; set; }
        public string LastName { get; set; }
        public string Shift { get; set; }
        public decimal? LongevityRate { get; set; }
        public DateTime? LongevityDate { get; set; }
        public string TitleCode { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime? HireDate { get; set; }
        public int? EmployeeTypeObjectID { get; set; }
        public string EmployeeTypeName { get; set; }
        public int? AccountObjectId { get; set; }
        public string AccountName { get; set; }
        public bool Active { get; set; }

        // Full SSN only ever populated on a single-record edit fetch, never in grid lists.
        public int? SSN { get; set; }

        // Current rate snapshot, sourced from EmployeeRate — grid display only, never edited here.
        public decimal? BaseRate { get; set; }
        public decimal? CurrentRate { get; set; }

        public string FullName
        {
            get
            {
                var mi = string.IsNullOrWhiteSpace(MI) ? "" : $" {MI}.";
                return $"{FirstName}{mi} {LastName}".Replace("  ", " ").Trim();
            }
        }
    }

    public class EmployeeIndexVM
    {
        public List<EmployeeVM> Employees { get; set; } = new List<EmployeeVM>();

        public string SearchTerm { get; set; }
        public bool ShowInactive { get; set; }
        public int? EmployeeTypeFilter { get; set; }

        public string SortField { get; set; } = "LastName";
        public string SortDirection { get; set; } = "asc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // Simple {id, label} pair for populating employee dropdowns (Add Rate modal, etc.)
    public class EmployeeOptionVM
    {
        public int ObjectID { get; set; }
        public string DisplayName { get; set; } // "Last, First (Code)"
    }
}
