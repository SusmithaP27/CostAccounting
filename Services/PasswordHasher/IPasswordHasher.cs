namespace CostAccounting.Services.PasswordHasher
{
    public interface IPasswordHasher
    {
        (string hash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, string storedHash, string storedSalt);
    }
}