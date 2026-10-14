using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CostAccounting.Models
{
    public class Paycode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short Code { get; set; }          // was: string?

        public string? Description { get; set; }
        public string? Abbrev { get; set; }
        public DateTime? EnteredDate { get; set; }
        public string? EnteredByUser { get; set; }
        public bool? Active { get; set; }
        public bool? OutOfClass { get; set; }
        public bool? ShiftDiff { get; set; }
        public bool? OPP { get; set; }
        public decimal Calculation { get; set; }
    }
}
