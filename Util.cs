using System;

public static class Util
{
    public static Random Rng = new Random();

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
}
