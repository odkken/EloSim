using System;
using System.Collections.Generic;
using System.Linq;

namespace ELO
{
    public static class Util
    {

        private static readonly Random rng = new Random();
        private static readonly object RngLock = new object();
        public static Random Rng
        {
            get
            {
                return rng;
            }
        }

        public static double GaussianRandom(double mean, double stdDev)
        {
            var u1 = Rng.NextDouble();
            var u2 = Rng.NextDouble();
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
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.ShuffleIterator();
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = Rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
        public static double NewElo(double myElo, double theirElo, double myPerformance, double k)
        {
            var diff = theirElo - myElo;
            if (Math.Abs(diff) > 400)
                diff = diff > 0 ? 400 : -400;
            var expected = 1 / (1 + Math.Pow(10, (diff / 400)));
            return myElo + k * (myPerformance - expected);
        }
    }
}
