using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class PasswordHasher
    {
        public string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool Verify(string password, string storedHash)
        {
            return Hash(password) == storedHash;
        }
    }
}
