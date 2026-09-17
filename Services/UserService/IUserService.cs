using CostAccounting.Models;

namespace CostAccounting.Services.UserService
{
    public interface IUserService
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int id);
        Task<List<UserIndexVM>> GetAllAsync();
        Task<(bool success, string message)> CreateUserAsync(CreateUserVM vm, string enteredByUser);
        Task<(bool success, string message)> AdminResetPasswordAsync(int targetUserId, string newPassword);
        Task<(bool success, string message)> SelfResetPasswordAsync(int userId, string currentPassword, string newPassword);
        Task UpdateLastLoginAsync(int userId);
    }

}