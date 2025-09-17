using MathNet.Numerics.Statistics;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

internal static class FirstOrderExpectedCalculator
{
    public static IReadOnlyDictionary<FirstOrderFeatureType, double> Compute(ImagePlus image, ImagePlus mask, ImagePlus discrete, CaculateParams parameters)
    {
        var voxels = Utils.GetVoxels(image, mask, (int)parameters.Label);
        var discreteVoxels = Utils.GetVoxels(discrete, mask, (int)parameters.Label);
        var histogram = Utils.GetHistogram(discreteVoxels)!;

        var volume = image.PixelWidth * image.PixelHeight * image.PixelDepth;
        var mean = Statistics.Mean(voxels);
        var variance = CalculateVariance(voxels, mean);
        var stdDev = Math.Sqrt(variance);
        var energy = voxels.Sum(v => Math.Pow(v + parameters.DensityShift, 2));
        var sorted = voxels;
        Array.Sort(sorted);

        var percentile10 = GetPercentile(sorted, 10);
        var percentile25 = GetPercentile(sorted, 25);
        var percentile75 = GetPercentile(sorted, 75);
        var percentile90 = GetPercentile(sorted, 90);

        var robustRange = sorted.Where(v => v >= percentile10 && v <= percentile90).ToArray();
        var robustMean = robustRange.Length > 0 ? Statistics.Mean(robustRange) : double.NaN;
        var robustMad = robustRange.Length > 0
            ? robustRange.Sum(v => Math.Abs(v - robustMean)) / robustRange.Length
            : double.NaN;

        var entropy = CalculateEntropy(histogram, voxels.Length);
        var uniformity = CalculateUniformity(histogram, voxels.Length);
        var peak = CalculatePeak(image, mask, (int)parameters.Label);

        return new Dictionary<FirstOrderFeatureType, double>
        {
            [FirstOrderFeatureType.Mean] = mean,
            [FirstOrderFeatureType.Variance] = variance,
            [FirstOrderFeatureType.Skewness] = CalculateSkewness(voxels, mean),
            [FirstOrderFeatureType.Kurtosis] = CalculateKurtosis(voxels, mean),
            [FirstOrderFeatureType.Median] = GetMedian(sorted),
            [FirstOrderFeatureType.Minimum] = sorted.First(),
            [FirstOrderFeatureType.Percentile10] = percentile10,
            [FirstOrderFeatureType.Percentile90] = percentile90,
            [FirstOrderFeatureType.Maximum] = sorted.Last(),
            [FirstOrderFeatureType.Peak] = peak,
            [FirstOrderFeatureType.Interquartile] = percentile75 - percentile25,
            [FirstOrderFeatureType.Range] = sorted.Last() - sorted.First(),
            [FirstOrderFeatureType.MeanAbsoluteDeviation] = voxels.Sum(v => Math.Abs(v - mean)) / voxels.Length,
            [FirstOrderFeatureType.RobustMeanAbsoluteDeviation] = robustMad,
            [FirstOrderFeatureType.Energy] = energy,
            [FirstOrderFeatureType.RootMeanSquared] = Math.Sqrt(energy / voxels.Length),
            [FirstOrderFeatureType.TotalEnergy] = energy * volume,
            [FirstOrderFeatureType.StandardDeviation] = stdDev,
            [FirstOrderFeatureType.Entropy] = entropy,
            [FirstOrderFeatureType.Uniformity] = uniformity
        };
    }

    private static double CalculateVariance(double[] voxels, double mean)
    {
        double sumsq = 0;
        foreach (var v in voxels)
        {
            sumsq += Math.Pow(v - mean, 2);
        }
        return sumsq / voxels.Length;
    }

    private static double CalculateSkewness(double[] voxels, double mean)
    {
        double sum2 = 0;
        double sum3 = 0;
        foreach (var v in voxels)
        {
            var diff = v - mean;
            sum2 += Math.Pow(diff, 2);
            sum3 += Math.Pow(diff, 3);
        }
        var n = voxels.Length;
        return (sum3 / n) / Math.Pow(Math.Sqrt(sum2 / n), 3);
    }

    private static double CalculateKurtosis(double[] voxels, double mean)
    {
        double sum2 = 0;
        double sum4 = 0;
        foreach (var v in voxels)
        {
            var diff = v - mean;
            sum2 += Math.Pow(diff, 2);
            sum4 += Math.Pow(diff, 4);
        }
        var n = voxels.Length;
        return (sum4 / n) / Math.Pow(sum2 / n, 2);
    }

    private static double GetMedian(double[] sorted)
    {
        if (sorted.Length % 2 == 0)
        {
            var mid = sorted.Length / 2;
            return (sorted[mid] + sorted[mid - 1]) / 2d;
        }
        return sorted[sorted.Length / 2];
    }

    private static double GetPercentile(double[] sorted, int percentile)
    {
        var index = (int)Math.Floor(percentile / 100d * sorted.Length);
        index = Math.Clamp(index, 0, sorted.Length - 1);
        return sorted[index];
    }

    private static double CalculateEntropy(int[] histogram, int total)
    {
        double entropy = 0;
        foreach (var count in histogram)
        {
            if (count <= 0)
            {
                continue;
            }
            var p = count / (double)total;
            entropy -= p * (Math.Log(p) / Math.Log(2d));
        }
        return entropy;
    }

    private static double CalculateUniformity(int[] histogram, int total)
    {
        double uniformity = 0;
        foreach (var count in histogram)
        {
            if (count <= 0)
            {
                continue;
            }
            var p = count / (double)total;
            uniformity += Math.Pow(p, 2);
        }
        return uniformity;
    }

    private static double CalculatePeak(ImagePlus image, ImagePlus mask, int label)
    {
        double peak = double.MinValue;
        for (int z = 0; z < image.Slice; z++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if ((int)mask.GetXYZ(x, y, z) != label)
                    {
                        continue;
                    }

                    var neighbours = new List<double>();
                    for (int nz = z - 1; nz <= z + 1; nz++)
                    {
                        if (nz < 0 || nz >= image.Slice)
                        {
                            continue;
                        }
                        for (int ny = y - 1; ny <= y + 1; ny++)
                        {
                            if (ny < 0 || ny >= image.Height)
                            {
                                continue;
                            }
                            for (int nx = x - 1; nx <= x + 1; nx++)
                            {
                                if (nx < 0 || nx >= image.Width)
                                {
                                    continue;
                                }
                                var distance = Math.Sqrt(Math.Pow(nx - x, 2) + Math.Pow(ny - y, 2) + Math.Pow(nz - z, 2));
                                if (distance == 0 || distance > 1)
                                {
                                    continue;
                                }
                                neighbours.Add(image.GetXYZ(nx, ny, nz));
                            }
                        }
                    }

                    double candidate;
                    if (neighbours.Count == 0)
                    {
                        candidate = image.GetXYZ(x, y, z);
                    }
                    else
                    {
                        candidate = neighbours.Average();
                    }

                    if (candidate > peak)
                    {
                        peak = candidate;
                    }
                }
            }
        }

        return peak == double.MinValue ? double.NaN : peak;
    }
}
