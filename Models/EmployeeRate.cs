using System;

namespace CostAccounting.Models
{
    public class EmployeeRate
    {
        public int ObjectID { get; set; }
        public int EmployeeObjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate { get; set; }
        public decimal? BaseRate { get; set; }
        public string EnteredByUser { get; set; }
        public DateTime EnteredDate { get; set; }
        public bool Deleted { get; set; }

        public Employee Employee { get; set; }
    }
}
