using System.ComponentModel.DataAnnotations;

namespace CostAccounting.Models
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class CreateUserVM
    {
        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; } // "Admin" / "User"
    }

    public class UserIndexVM
    {
        public int ObjectID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class ResetPasswordVM
    {
        public int TargetUserObjectID { get; set; } // admin-only; ignored for self-service
        public string CurrentPassword { get; set; }  // required for self-service only

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileVM
    {
        public int ObjectID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
     public class UsersIndexPageVM
    {
        public List<UserIndexVM> Users { get; set; } = new List<UserIndexVM>();

        public string SearchTerm { get; set; }
        public bool ShowInactive { get; set; }

        public string SortField { get; set; } = "Username";
        public string SortDirection { get; set; } = "asc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // NOTE: LoginVM is NOT defined here — it already exists in your app (used by
    // User_AccountController), so redefining it here would be a duplicate-type compile error.

    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
