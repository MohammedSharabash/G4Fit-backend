using System;
using System.Linq;

namespace G4Fit.Helpers
{
    public static class RandomGenerator
    {
        private static Random random = new Random();
        public static string GenerateString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int GenerateNumber(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}