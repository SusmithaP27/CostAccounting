using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Equipment
    {
        [Key]
        public int ObjectID { get; set; }

        public string? Code { get; set; }

        public string? Category { get; set; }

        public string? MfgYear { get; set; }

        public DateTime? DateInService { get; set; }

        public string? Make { get; set; }

        public string? Model { get; set; }

        public string? Department { get; set; }

        public string? EnteredByUser { get; set; }

        public DateTime? EnteredDate { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }
    }
}
