namespace CostAccounting.Models
{
    public class User
    {
        public int ObjectID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public bool MustChangePassword { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime EnteredDate { get; set; }
        public string EnteredByUser { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

}
