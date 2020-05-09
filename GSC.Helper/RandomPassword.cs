using System;

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
    }
}