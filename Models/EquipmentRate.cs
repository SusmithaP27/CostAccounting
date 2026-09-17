using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class EquipmentRate
    {

        [Key]
        public int ObjectID { get; set; }

        public int EquipmentObjectId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal MiscRate { get; set; }

        public decimal Rate { get; set; }

        public string? EnteredByUser { get; set; }

        public DateTime? EnteredDate { get; set; }

        public bool? Deleted { get; set; }
    }
}
