// namespace CostAccounting.Models
// {
//     // Maps 1:1 to the columns returned by usp_GetEmployees
//     public class EmployeeVM
//     {
//         public int EmployeeID { get; set; }
//         public string? LastName { get; set; }
//         public string? FirstName { get; set; }
//         public DateTime? HireDate { get; set; }
//         public DateTime? LongDate { get; set; }
//         public string? Status { get; set; }
//         public decimal? Rate { get; set; }

//         public string FullName => $"{LastName}, {FirstName}".Trim(' ', ',');
//     }

//     public class EmployeeIndexVM
//     {
//         // Table data
//         public IEnumerable<EmployeeVM> Employees { get; set; } = [];

//         // Filters
//         public string? SearchTerm { get; set; }

//         // Sort
//         public string? SortColumn { get; set; }
//         public string? SortDir { get; set; }

//         // Pagination
//         public int CurrentPage { get; set; } = 1;
//         public int PageSize { get; set; } = 25;
//         public int TotalCount { get; set; }
//         public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
//     }
// }

using System;
using System.Collections.Generic;

namespace CostAccounting.Models
{
    public class EmployeeVM
    {
        public int ObjectID { get; set; }
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
