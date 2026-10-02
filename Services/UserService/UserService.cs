using CostAccounting.Data;
using CostAccounting.Models;
using CostAccounting.Services.PasswordHasher;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.UserService
{
    public class UserService : IUserService
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

        // Safe-for-client projection (no PasswordHash/Salt) for the Edit User modal.
        public async Task<UserIndexVM> GetByIdVMAsync(int id)
        {
            return await _context.Login_Users
                .Where(u => u.ObjectID == id && !u.Deleted)
                .Select(u => new UserIndexVM
                {
                    ObjectID = u.ObjectID,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    Active = u.Active,
                    LastLoginDate = u.LastLoginDate
                })
                .FirstOrDefaultAsync();
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

        public async Task<UsersIndexPageVM> GetIndexAsync(UsersIndexPageVM filter)
        {
            var query = _context.Login_Users
                .Where(u => !u.Deleted)
                .AsQueryable();

            if (!filter.ShowInactive)
                query = query.Where(u => u.Active);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(u =>
                    u.Username.Contains(term) ||
                    u.FullName.Contains(term) ||
                    u.Email.Contains(term));
            }

            filter.TotalCount = await query.CountAsync();

            query = filter.SortField switch
            {
                "FullName" => filter.SortDirection == "asc" ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                "Email" => filter.SortDirection == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "Role" => filter.SortDirection == "asc" ? query.OrderBy(u => u.Role) : query.OrderByDescending(u => u.Role),
                "LastLoginDate" => filter.SortDirection == "asc" ? query.OrderBy(u => u.LastLoginDate) : query.OrderByDescending(u => u.LastLoginDate),
                _ => filter.SortDirection == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username),
            };

            filter.Users = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserIndexVM
                {
                    ObjectID = u.ObjectID,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    Active = u.Active,
                    LastLoginDate = u.LastLoginDate
                })
                .ToListAsync();

            return filter;
        }

        public async Task<(bool success, string message)> CreateUserAsync(CreateUserVM vm, string enteredByUser)
        {
            // var exists = await _context.Login_Users.AnyAsync(u => u.Username == vm.Username && u.Email == vm.Email);
            var exists = await _context.Login_Users.FirstOrDefaultAsync(u => u.Username == vm.Username || u.Email == vm.Email);
            if (exists != null)
            {
                if (exists.Username == vm.Username)
                    return (false, "Username already exists.");

                if (exists.Email == vm.Email)
                    return (false, "Email already exists.");
            }

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

        // Username is intentionally never reassigned here (matches the original behavior) —
        // it's fixed at creation. The duplicate check below only guards the case where a
        // caller submits a different username than the one on file; the Edit modal keeps the
        // Username field disabled so this path isn't normally reachable from the UI.
        public async Task<(bool Success, string Message)> UpdateUserAsync(UserIndexVM model)
        {
            var user = await _context.Login_Users.FindAsync(model.ObjectID);
            if (user == null)
                return (false, "Account not found.");

            var exists = await _context.Login_Users
                .AnyAsync(a => a.Username == model.Username &&
                               a.ObjectID != model.ObjectID &&
                               a.Deleted != true);

            if (exists)
                return (false, $"Another account with code '{model.Username}' already exists.");

            user.FullName = model.FullName?.Trim();
            user.Email = model.Email;
            user.Role = model.Role;
            user.Active = model.Active;

            await _context.SaveChangesAsync();
            return (true, "User updated.");
        }

        // Soft delete only — sets Deleted = 1, never removes the row.
        public async Task<(bool Success, string Message)> SoftDeleteAsync(int objectId)
        {
            var user = await _context.Login_Users.FindAsync(objectId);
            if (user == null)
                return (false, "Account not found.");

            user.Deleted = true;
            await _context.SaveChangesAsync();
            return (true, "User deleted successfully.");
        }
    }
}
