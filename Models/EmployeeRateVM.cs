using System;
using System.Collections.Generic;

namespace CostAccounting.Models
{
    public class EmployeeRateVM
    {
        public int ObjectID { get; set; }
        public int EmployeeObjectId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
        public decimal? BaseRate { get; set; }

        // Current = today is the start date, the end date, or anywhere in between.
        // A blank end date means open-ended.
        public bool IsCurrent =>
            StartDate.Date <= DateTime.Today &&
            (!EndDate.HasValue || EndDate.Value.Date >= DateTime.Today);

        // Scheduled = the row exists but its start date hasn't arrived yet.
        public bool IsScheduled => StartDate.Date > DateTime.Today;
    }

    public class EmployeeRateIndexVM
    {
        public List<EmployeeRateVM> EmployeeRates { get; set; } = new List<EmployeeRateVM>();

        public string SearchTerm { get; set; }
        public int? EmployeeObjectId { get; set; } // filter to one employee's rate history
        // Status filter: "current" (default), "scheduled", "ended" or "all"
        public string Status { get; set; } = "current";
        // public bool ShowInactiveEmployees { get; set; } // include rates belonging to inactive employees
        public DateTime? DateRangeStart { get; set; }
        public DateTime? DateRangeEnd { get; set; }

        public string SortField { get; set; } = "StartDate";
        public string SortDirection { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
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

    // Payload for the adjustment modal.
    //   IsSeasonal   : true  = seasonal (changes Rate only, uses StartDate..EndDate)
    //                  false = annual   (changes Rate AND BaseRate, uses StartDate as the effective date, open-ended)
    //   Percent      : 10 = the standard seasonal swing; any other value is a custom percentage.
    //   SelectedOnly : true = apply only to EmployeeObjectIds (never falls back to "everyone").
    public class SeasonalAdjustmentRequest
    {
        public bool IsSeasonal { get; set; }
        public bool IsIncrease { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Percent { get; set; } = 10m;
        public bool SelectedOnly { get; set; }
        public List<int> EmployeeObjectIds { get; set; }
    }
}
