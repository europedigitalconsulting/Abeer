using System;
using System.Security.Cryptography;

namespace Abeer.Shared
{
    public static class KeyGenerator
    {
        private static readonly Random random = new Random();

        public static int GeneratePinCode(int maxLength)
        {
            var min = "1".PadRight(maxLength);
            var max = "9".PadRight(maxLength, '9');

            return random.Next(int.Parse(min), int.Parse(max));
        }

        public static byte[] GetRandomData(int size)
        {
            var result = new byte[size / 8];
            RandomNumberGenerator.Create().GetBytes(result);
            return result;
        }
    }
}
