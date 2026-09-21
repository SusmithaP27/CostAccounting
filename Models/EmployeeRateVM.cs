using System;
using System.Collections.Generic;

namespace CostAccounting.Models
{
    public class EmployeeRateVM
    {
        public int ObjectID { get; set; }
        public int EmployeeObjectId { get; set; }
        public string EmployeeName { get; set; } // resolved via join, like Material name on MaterialRate rows
        public string EmployeeCode { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
        public decimal? BaseRate { get; set; }

        public bool IsCurrent => !EndDate.HasValue || EndDate.Value >= DateTime.Today;
    }

    public class EmployeeRateIndexVM
    {
        public List<EmployeeRateVM> EmployeeRates { get; set; } = new List<EmployeeRateVM>();

        public string SearchTerm { get; set; }
        public int? EmployeeObjectId { get; set; } // filter to one employee's rate history
        public bool ShowAll { get; set; } // include end-dated rate rows
        public bool ShowInactiveEmployees { get; set; } // include rates belonging to inactive employees
        public DateTime? DateRangeStart { get; set; }
        public DateTime? DateRangeEnd { get; set; }

        public string SortField { get; set; } = "StartDate";
        public string SortDirection { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Populated only on the initial Index page load, for the Add Rate modal's dropdown.
        public List<EmployeeOptionVM> EmployeeOptions { get; set; } = new List<EmployeeOptionVM>();
    }

    // Payload for the bulk end-date action, matching the Equipment/MaterialRate pattern
    public class BulkEndDateRequest
    {
        public List<int> ObjectIDs { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Payload for the seasonal +/-10% adjustment. EmployeeObjectIds empty/omitted means
    // "apply to every active employee with a current rate."
    public class SeasonalAdjustmentRequest
    {
        public bool IsIncrease { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<int> EmployeeObjectIds { get; set; }
    }
}
