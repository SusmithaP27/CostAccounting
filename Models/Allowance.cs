using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Allowance
    {
        [Key]
        public int ObjectID { get; set; }

        public int? AllowanceTypeObjectID { get; set; }

        public short? Year { get; set; }

        public decimal? Rate { get; set; }

        public int? AllowanceSubTypeObjectID { get; set; }

        public DateTime? EnteredDate { get; set; }

        public string? EnteredByUser { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }
    }
}
