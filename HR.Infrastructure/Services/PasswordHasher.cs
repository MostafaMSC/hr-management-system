using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using HR.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using HR.Application.Common.Interfaces;

namespace HR.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            // âœ… Ensure password is not null
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty");

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            var hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            // âœ… Null checks
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (string.IsNullOrEmpty(hash)) return false;

            byte[] hashBytes;
            try
            {
                hashBytes = Convert.FromBase64String(hash);
            }
            catch (FormatException)
            {
                // âŒ Stored hash is not valid Base64 - database corruption?
                return false;
            }

            // âœ… Length validation with tolerance for different sizes
            if (hashBytes.Length < SaltSize + KeySize) return false;

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var computedHash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            // âœ… FIXED: Ø§Ø³ØªØ®Ø¯Ø§Ù… FixedTimeEquals Ù„Ù…Ù†Ø¹ timing attacks
            var storedHash = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, KeySize);
            
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}
