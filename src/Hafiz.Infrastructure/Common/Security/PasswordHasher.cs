using Hafiz.Application.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace Hafiz.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Generate a random salt
            return Argon2.Hash(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return Argon2.Verify(hash, password);
        }
    }
}
