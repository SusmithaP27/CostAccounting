using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Operation
    {
        [Key]
        public int ObjectID { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public DateTime? EnteredDate { get; set; }

        public string? EnteredByUser { get; set; }

        public bool? Engineering { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }
    }
}
