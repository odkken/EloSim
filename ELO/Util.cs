using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ELO
{
    public static class Util
    {

        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random _local;

            public static Random ThisThreadsRandom
            {
                get { return _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }
        public static double NextDouble()
        {
            return ThreadSafeRandom.ThisThreadsRandom.NextDouble();
        }
        public static int Next(int minValue, int maxValue)
        {
            return ThreadSafeRandom.ThisThreadsRandom.Next(minValue, maxValue);
        }
        public static int Next(int maxValue)
        {
            return Next(0, maxValue);
        }
        public static double KFactor(double elo)
        {
            if (elo > 2500)
                return 16;
            if (elo > 2200)
                return 24;
            return 32;
        }
        public static double GaussianRandom(double mean, double stdDev)
        {
            var u1 = NextDouble();
            var u2 = NextDouble();
            var randStdNormal = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val > max)
                return max;
            if (val < min)
                return min;
            return val;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void CrappyShuffle<T>(this IList<T> list, int delta)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Math.Max(Next(n - delta, n + 1), 0);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static double EloChange(double myElo, double theirElo, double myPerformance, double k)
        {
            var diff = theirElo - myElo;
            if (Math.Abs(diff) > 400)
                diff = diff > 0 ? 400 : -400;
            var expected = 1 / (1 + Math.Pow(10, (diff / 400)));
            var ratingChange = k * (myPerformance - expected);
            return ratingChange;
        }
    }
}
