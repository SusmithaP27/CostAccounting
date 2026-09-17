using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Lookup
    {
        [Key]
        public int ObjectID { get; set; }

        public byte LookupCategoryObjectID { get; set; }

        [StringLength(150)]
        public string? Code { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }

        public byte? DisplayOrder { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        [StringLength(50)]
        public string? EnteredByUser { get; set; }

        public DateTime? EnteredDate { get; set; }
    }
}
