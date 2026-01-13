using System.Security.Cryptography;
using System.Text;

namespace ClothesShop.Models
{
    public class MomoHelper
    {
        public static string HmacSHA256(string rawData, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);
            return BitConverter.ToString(hmac.ComputeHash(messageBytes))
                .Replace("-", "")
                .ToLower();
        }
    }
}
