using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class Material
    {
        [Key]
        public int ObjectID { get; set; }

        public DateTime? EnteredDate { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public string? VendorName { get; set; }

        public string? ContractNum { get; set; }

        public int? UnitTypeObjectID { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }
    }
}
