using System.Security.Cryptography;
using System.Text;

namespace LibraryTrackerApp.Services
{
    public static class PasswordService
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
