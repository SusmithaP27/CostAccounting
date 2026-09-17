using CostAccounting.Data;
using CostAccounting.Models;
using CostAccounting.Services.PasswordHasher;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.UserService
{public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _hasher;

        public UserService(ApplicationDbContext context, IPasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Login_Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.Deleted && u.Active);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Login_Users.FirstOrDefaultAsync(u => u.ObjectID == id && !u.Deleted);
        }

        public async Task<List<UserIndexVM>> GetAllAsync()
        {
            return await _context.Login_Users
                .Where(u => !u.Deleted)
                .OrderBy(u => u.Username)
                .Select(u => new UserIndexVM
                {
                    ObjectID = u.ObjectID,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    Active = u.Active,
                    LastLoginDate = u.LastLoginDate
                }).ToListAsync();
        }

        public async Task<(bool success, string message)> CreateUserAsync(CreateUserVM vm, string enteredByUser)
        {
            var exists = await _context.Login_Users.AnyAsync(u => u.Username == vm.Username && !u.Deleted);
            if (exists)
                return (false, "Username already exists.");

            var (hash, salt) = _hasher.HashPassword(vm.Password);

            var user = new User
            {
                Username = vm.Username,
                Email = vm.Email,
                FullName = vm.FullName,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = vm.Role,
                MustChangePassword = true, // force change on first login
                Active = true,
                Deleted = false,
                EnteredDate = DateTime.Now,
                EnteredByUser = enteredByUser
            };

            _context.Login_Users.Add(user);
            await _context.SaveChangesAsync();
            return (true, "User created successfully.");
        }

        public async Task<(bool success, string message)> AdminResetPasswordAsync(int targetUserId, string newPassword)
        {
            var user = await GetByIdAsync(targetUserId);
            if (user == null) return (false, "User not found.");

            var (hash, salt) = _hasher.HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.MustChangePassword = true; // force user to set their own password next login

            await _context.SaveChangesAsync();
            return (true, "Password reset successfully.");
        }

        public async Task<(bool success, string message)> SelfResetPasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            if (!_hasher.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                return (false, "Current password is incorrect.");

            var (hash, salt) = _hasher.HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.MustChangePassword = false;

            await _context.SaveChangesAsync();
            return (true, "Password changed successfully.");
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Login_Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
