using System;
using System.Linq;

namespace YouRock
{
    public class RandomHelper
    {
        private static readonly object SyncLock = new object();
        private static readonly Random Random = new Random();

        public static string String(int length)
        {
            lock (SyncLock)
            {
                return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select(s => s[Random.Next(s.Length)]).ToArray());
            }
        }

        public static int Number(int min, int max)
        {
            lock (SyncLock)
            {
                return Random.Next(min, ++max);
            }
        }
    }
}