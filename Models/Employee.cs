using System;
using System.Collections.Generic;

namespace CostAccounting.Models
{
    public class Employee
    {
        public int ObjectID { get; set; }

        public string? Code { get; set; }

        public int? MunisCode { get; set; }

        public string? FirstName { get; set; }

        public string? MI { get; set; }

        public string? LastName { get; set; }

        public string? Shift { get; set; }

        public decimal? LongevityRate { get; set; }

        public DateTime? LongevityDate { get; set; }

        public string? TitleCode { get; set; }

        public DateTime? TerminationDate { get; set; }

        public DateTime? HireDate { get; set; }

        public int? EmployeeTypeObjectID { get; set; }

        public int? AccountObjectId { get; set; }

        public string? EnteredByUser { get; set; }

        public DateTime EnteredDate { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        public ICollection<EmployeeRate> EmployeeRates { get; set; }
            = new List<EmployeeRate>();
    }
}