using System;
using System.Security.Cryptography;
using System.Text;

namespace GSC.Helper
{
    public static class RandomPassword
    {
        public static string CreateRandomPassword(int passwordLength)
        {
            var allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            var randNum = new Random();
            var chars = new char[passwordLength];
            var allowedCharCount = allowedChars.Length;
            for (var i = 0; i < passwordLength; i++)
                chars[i] = allowedChars[(int) (allowedChars.Length * randNum.NextDouble())];
            return new string(chars);
        }
        public static string CreateRandomNumericNumber(int len)
        {
            int maxSize = len;
            char[] chars = new char[30];
            string a;
            a = "1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data) { result.Append(chars[b % (chars.Length)]); }
            return result.ToString();
        }
    }
}