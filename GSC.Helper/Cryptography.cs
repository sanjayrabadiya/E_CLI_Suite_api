using System;
using System.Security.Cryptography;
using System.Text;

namespace GSC.Helper
{
    public class Cryptography
    {
        private static readonly int _saltKeySize = 16;

        public static string CreateSaltKey()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var buff = new byte[_saltKeySize];
                provider.GetBytes(buff);
                return Convert.ToBase64String(buff);
            }
        }

        public static string CreatePasswordHash(string password, string saltkey)
        {
            return CreateHash(Encoding.UTF8.GetBytes(string.Concat(password, saltkey)));
        }

        public static bool ValidatePassword(string hasedPassword, string plainPassword, string saltKey)
        {
            return hasedPassword == CreatePasswordHash(plainPassword, saltKey);
        }

        private static string CreateHash(byte[] data)
        {
            var algorithm = (HashAlgorithm) CryptoConfig.CreateFromName("SHA512");
            if (algorithm == null)
                throw new ArgumentException("Unrecognized hash name");

            var hashByteArray = algorithm.ComputeHash(data);
            return BitConverter.ToString(hashByteArray).Replace("-", string.Empty);
        }
    }
}