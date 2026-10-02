using CostAccounting.Models;

namespace CostAccounting.Services.UserService
{
    public interface IUserService
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int id);

        // Safe-for-client projection of a single user (no password hash/salt), for populating
        // the Edit User modal.
        Task<UserIndexVM> GetByIdVMAsync(int id);

        Task<List<UserIndexVM>> GetAllAsync();

        // Search/filter/sort/paginate, matching the Employee grid's pattern.
        Task<UsersIndexPageVM> GetIndexAsync(UsersIndexPageVM filter);

        Task<(bool success, string message)> CreateUserAsync(CreateUserVM vm, string enteredByUser);
        Task<(bool success, string message)> AdminResetPasswordAsync(int targetUserId, string newPassword);
        Task<(bool success, string message)> SelfResetPasswordAsync(int userId, string currentPassword, string newPassword);
        Task UpdateLastLoginAsync(int userId);

        // Renamed from the original UpdateuserAsync for casing consistency with the rest of
        // the app (Employee/EmployeeRate services use PascalCase "Update...Async").
        Task<(bool Success, string Message)> UpdateUserAsync(UserIndexVM model);

        // Soft delete only — sets Deleted = 1, same pattern as Employee. A separate,
        // non-destructive way to lock someone out without hiding them from the admin list is
        // the Active flag on UpdateUserAsync (unchecking "Active" in the Edit modal).
        Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
    }
}
