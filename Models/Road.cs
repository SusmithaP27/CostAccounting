using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Road
    {
        [Key]
        public short ObjectID { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public int? RoadTypeObjectID { get; set; }

        public string? EnteredByUser { get; set; }

        public DateTime? EnteredDate { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }

        public bool? Engineering { get; set; }
    }
}
