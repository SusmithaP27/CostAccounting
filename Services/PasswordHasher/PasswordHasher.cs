using System.Security.Cryptography;
using CostAccounting.Services.PasswordHasher;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CostAccounting.Services.PasswordHasher
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8;
        private const int HashSize = 256 / 8;
        private const int Iterations = 100_000;

        public (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes);

            var hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            // Constant-time comparison to avoid timing attacks
            return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(storedHash));
        }
    }
}
