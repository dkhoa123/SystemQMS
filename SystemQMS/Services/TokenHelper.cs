using System.Security.Cryptography;
using System.Text;

namespace SystemQMS.Services
{
    // bam jwt token
    public static class TokenHelper
    {
        public static string HashToken (string token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
