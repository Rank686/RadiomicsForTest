using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Statistics;

public static class Statistics
{
    public static double Minimum(IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var enumerable = source as double[] ?? source.ToArray();
        if (enumerable.Length == 0) throw new InvalidOperationException("Sequence contains no elements.");
        var min = enumerable[0];
        for (var i = 1; i < enumerable.Length; i++)
        {
            if (enumerable[i] < min)
            {
                min = enumerable[i];
            }
        }
        return min;
    }

    public static double Maximum(IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var enumerable = source as double[] ?? source.ToArray();
        if (enumerable.Length == 0) throw new InvalidOperationException("Sequence contains no elements.");
        var max = enumerable[0];
        for (var i = 1; i < enumerable.Length; i++)
        {
            if (enumerable[i] > max)
            {
                max = enumerable[i];
            }
        }
        return max;
    }

    public static double Mean(IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var enumerable = source as double[] ?? source.ToArray();
        if (enumerable.Length == 0) throw new InvalidOperationException("Sequence contains no elements.");
        var sum = 0d;
        for (var i = 0; i < enumerable.Length; i++)
        {
            sum += enumerable[i];
        }
        return sum / enumerable.Length;
    }

    public static double Mean(double[] source) => Mean((IEnumerable<double>)source);

    public static double Variance(IEnumerable<double> source) => StatisticsExtensions.Variance(source);

    public static double Variance(double[] source) => StatisticsExtensions.Variance(source);

    public static double Percentile(double[] source, double percentile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (source.Length == 0) throw new InvalidOperationException("Sequence contains no elements.");
        var data = (double[])source.Clone();
        Array.Sort(data);
        var position = percentile / 100.0 * (data.Length - 1);
        var lower = (int)Math.Floor(position);
        var upper = (int)Math.Ceiling(position);
        if (lower == upper)
        {
            return data[lower];
        }
        var weight = position - lower;
        return data[lower] * (1 - weight) + data[upper] * weight;
    }

    public static double Percentile(IEnumerable<double> source, double percentile) =>
        Percentile(source.ToArray(), percentile);

    public static double Percentile(IEnumerable<double> source, int percentile) =>
        Percentile(source, (double)percentile);
}

public static class StatisticsExtensions
{
    public static double Minimum(this IEnumerable<double> source) => Statistics.Minimum(source);

    public static double Maximum(this IEnumerable<double> source) => Statistics.Maximum(source);

    public static double Mean(this IEnumerable<double> source) => Statistics.Mean(source);

    public static double Variance(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var data = source as double[] ?? source.ToArray();
        if (data.Length == 0) return double.NaN;
        var mean = Statistics.Mean(data);
        var sum = 0d;
        for (var i = 0; i < data.Length; i++)
        {
            var diff = data[i] - mean;
            sum += diff * diff;
        }
        return sum / data.Length;
    }

    public static double Mean(this double[] source) => Statistics.Mean(source);

    public static double Variance(this double[] source) => Variance((IEnumerable<double>)source);

    public static double Maximum(this double[] source) => Statistics.Maximum(source);

    public static double Minimum(this double[] source) => Statistics.Minimum(source);
}
