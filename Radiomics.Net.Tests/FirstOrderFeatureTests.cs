using System;
using System.Collections.Generic;
using System.Linq;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;
using Xunit;

namespace Radiomics.Net.Tests;

public sealed class FirstOrderFeatureTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void FirstOrderFeaturesShouldComputeExpectedStatistics()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            Label = 1,
            UseFixedBinNumber = true,
            NBins = 4,
            DensityShift = 0,
            EnableFirstOrder = true
        };

        var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);
        var features = new FirstOrderFeatures(image, mask, discretised, parameters);

        var expected = new Dictionary<FirstOrderFeatureType, double>
        {
            [FirstOrderFeatureType.Mean] = 2.5,
            [FirstOrderFeatureType.Median] = 2.5,
            [FirstOrderFeatureType.Minimum] = 1.0,
            [FirstOrderFeatureType.Maximum] = 4.0,
            [FirstOrderFeatureType.Range] = 3.0,
            [FirstOrderFeatureType.Variance] = 1.25,
            [FirstOrderFeatureType.StandardDeviation] = Math.Sqrt(1.25),
            [FirstOrderFeatureType.MeanAbsoluteDeviation] = 1.0,
            [FirstOrderFeatureType.RobustMeanAbsoluteDeviation] = 0.5,
            [FirstOrderFeatureType.Energy] = 30.0,
            [FirstOrderFeatureType.TotalEnergy] = 30.0,
            [FirstOrderFeatureType.RootMeanSquared] = Math.Sqrt(7.5),
            [FirstOrderFeatureType.Entropy] = 2.0,
            [FirstOrderFeatureType.Uniformity] = 0.25,
            [FirstOrderFeatureType.Percentile10] = 1.0,
            [FirstOrderFeatureType.Percentile90] = 4.0,
            [FirstOrderFeatureType.Interquartile] = 2.0,
            [FirstOrderFeatureType.Peak] = 2.5,
            [FirstOrderFeatureType.Skewness] = 0.0,
            [FirstOrderFeatureType.Kurtosis] = 1.64
        };

        foreach (var kvp in expected)
        {
            var actual = features.Calculate(kvp.Key);
            TestAssert.AreEqual(kvp.Value, actual, Tolerance, $"First order feature {kvp.Key} mismatch.");
        }
    }

    [Fact]
    public void FirstOrderFeaturesShouldUseHistogramWhenBinsProvided()
    {
        var data = new double[,] { { 2, 2 }, { 4, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            Label = 1,
            UseFixedBinNumber = true,
            NBins = 2,
            DensityShift = 0
        };

        var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);
        var features = new FirstOrderFeatures(image, mask, discretised, parameters);

        var uniformity = features.Calculate(FirstOrderFeatureType.Uniformity);
        TestAssert.AreEqual(0.5, uniformity, Tolerance, "Uniformity should reflect two equally likely bins.");
    }
}
