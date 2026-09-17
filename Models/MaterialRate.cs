using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class MaterialRate
    {
        [Key]
        public int ObjectID { get; set; }

        public int MaterialObjectId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal Rate { get; set; }

        public string? EnteredByUser { get; set; }

        public DateTime? EnteredDate { get; set; }

        public bool? Deleted { get; set; }
    }
}
