using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Utility.Appliation.Security
{
    public class Sha256Hasher
    {
        public static string Hash(string inputValue)
        {
            using var sha256 = SHA256.Create();
            var originalBytes = Encoding.Default.GetBytes(inputValue);
            var encodedBytes = sha256.ComputeHash(originalBytes);
            return Convert.ToBase64String(encodedBytes);
        }
    }
}
