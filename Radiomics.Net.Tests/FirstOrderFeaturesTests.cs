using System;
using System.Collections.Generic;
using Radiomics.Net;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;
using Xunit;

namespace Radiomics.Net.Tests
{
    public class FirstOrderFeaturesTests
    {
        private static (ImagePlus image, ImagePlus mask) CreateUniformMaskImage()
        {
            double[,] values =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 }
            };
            var image = new ImagePlus(3, 3, 1)
            {
                PixelWidth = 1,
                PixelHeight = 1,
                PixelDepth = 1,
                BitsAllocated = 16
            };
            var mask = new ImagePlus(3, 3, 1)
            {
                PixelWidth = 1,
                PixelHeight = 1,
                PixelDepth = 1,
                BitsAllocated = 16
            };
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    image.SetXYZ(x, y, 0, values[y, x]);
                    mask.SetXYZ(x, y, 0, 1);
                }
            }
            return (image, mask);
        }

        [Fact]
        public void Calculate_AllFirstOrderFeatures_MatchExpectedValues()
        {
            var (image, mask) = CreateUniformMaskImage();
            var parameters = new CaculateParams
            {
                Label = 1,
                UseFixedBinNumber = true,
                NBins = 9,
                DensityShift = 0
            };
            var discImg = Utils.Discrete(image, mask, (int)parameters.Label, parameters.NBins);
            var features = new FirstOrderFeatures(image, mask, discImg, parameters);

            var expected = new Dictionary<FirstOrderFeatureType, double>
            {
                [FirstOrderFeatureType.Mean] = 5.0,
                [FirstOrderFeatureType.Variance] = 6.666666666666667,
                [FirstOrderFeatureType.Skewness] = 0.0,
                [FirstOrderFeatureType.Kurtosis] = 1.7699999999999998,
                [FirstOrderFeatureType.Median] = 5.0,
                [FirstOrderFeatureType.Minimum] = 1.0,
                [FirstOrderFeatureType.Percentile10] = 1.0,
                [FirstOrderFeatureType.Percentile90] = 9.0,
                [FirstOrderFeatureType.Maximum] = 9.0,
                [FirstOrderFeatureType.Peak] = 7.0,
                [FirstOrderFeatureType.Interquartile] = 4.0,
                [FirstOrderFeatureType.Range] = 8.0,
                [FirstOrderFeatureType.MeanAbsoluteDeviation] = 2.2222222222222223,
                [FirstOrderFeatureType.RobustMeanAbsoluteDeviation] = 2.2222222222222223,
                [FirstOrderFeatureType.Energy] = 285.0,
                [FirstOrderFeatureType.RootMeanSquared] = 5.627314338711377,
                [FirstOrderFeatureType.TotalEnergy] = 285.0,
                [FirstOrderFeatureType.StandardDeviation] = 2.581988897471611,
                [FirstOrderFeatureType.Entropy] = 3.169925001442312,
                [FirstOrderFeatureType.Uniformity] = 0.1111111111111111
            };

            foreach (var featureType in Enum.GetValues<FirstOrderFeatureType>())
            {
                double result = features.Calculate(featureType);
                Assert.True(expected.ContainsKey(featureType));
                Assert.InRange(result - expected[featureType], -1e-6, 1e-6);
            }
        }
    }
}
