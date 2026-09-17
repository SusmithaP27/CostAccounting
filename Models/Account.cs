using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CostAccounting.Models
{
    [Table("Account", Schema = "dbo")]
    public class Account
    {
        [Key]
        public int ObjectID { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public DateTime? EnteredDate { get; set; }

        public string? EnteredByUser { get; set; }

        public int AccountTypeObjectID { get; set; }

        public int AgencyTypeObjectID { get; set; }

        public bool? Active { get; set; }

        public bool? Deleted { get; set; }

    }
}
