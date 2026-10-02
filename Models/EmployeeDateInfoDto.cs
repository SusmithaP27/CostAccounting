namespace CostAccounting.Models
{
    // Maps the result set of the GetEmployeeDate stored procedure. Keyless — registered with
    // .HasNoKey().ToView(null) in OnModelCreating so EF only uses it for raw SQL, never tries
    // to map it to an actual table.
    public class EmployeeDateInfoDto
    {
        public int? MunisCode { get; set; }
        public System.DateTime? LongevityDate { get; set; }
    }
}
