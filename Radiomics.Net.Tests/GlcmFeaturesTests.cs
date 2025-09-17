using System;
using System.Collections.Generic;
using Radiomics.Net;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;
using Xunit;

namespace Radiomics.Net.Tests
{
    public class GlcmFeaturesTests
    {
        private static (ImagePlus image, ImagePlus mask) CreateCheckerboardImage()
        {
            double[,] values =
            {
                { 1, 2, 1 },
                { 2, 1, 2 },
                { 1, 2, 1 }
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
        public void Calculate_AllGlcmFeatures_MatchExpectedValues()
        {
            var (image, mask) = CreateCheckerboardImage();
            var parameters = new CaculateParams
            {
                Label = 1,
                UseFixedBinNumber = true,
                NBins = 2,
                GLCMDelta = 1
            };
            var discImg = Utils.Discrete(image, mask, (int)parameters.Label, parameters.NBins);
            var glcmFeatures = new GLCMFeatures(image, mask, discImg, parameters);

            var expected = new Dictionary<GLCMFeatureType, double>
            {
                [GLCMFeatureType.MaximumProbability] = 0.5,
                [GLCMFeatureType.JointAverage] = 1.5,
                [GLCMFeatureType.SumSquares] = 0.25,
                [GLCMFeatureType.JointEntropy] = 1.0,
                [GLCMFeatureType.JointEnergy] = 0.5,
                [GLCMFeatureType.DifferenceAverage] = 0.5,
                [GLCMFeatureType.DifferenceVariance] = 0.0,
                [GLCMFeatureType.DifferenceEntropy] = 0.0,
                [GLCMFeatureType.SumAverage] = 3.0,
                [GLCMFeatureType.SumVariance] = 0.5,
                [GLCMFeatureType.SumEntropy] = 0.5,
                [GLCMFeatureType.Contrast] = 0.5,
                [GLCMFeatureType.InverseDifference] = 0.75,
                [GLCMFeatureType.NormalizedInverseDifference] = 0.8333333333333333,
                [GLCMFeatureType.InverseDifferenceMoment] = 0.75,
                [GLCMFeatureType.NormalizedInverseDifferenceMoment] = 0.9,
                [GLCMFeatureType.InverseVariance] = 0.5,
                [GLCMFeatureType.Correlation] = 0.0,
                [GLCMFeatureType.Autocorrection] = 2.25,
                [GLCMFeatureType.ClusterTendency] = 0.5,
                [GLCMFeatureType.ClusterShade] = 0.0,
                [GLCMFeatureType.ClusterProminence] = 0.5,
                [GLCMFeatureType.InformationalMeasureOfCorrelation1] = -1.0,
                [GLCMFeatureType.InformationalMeasureOfCorrelation2] = 0.0,
                [GLCMFeatureType.MCC] = 1.0
            };

            foreach (var featureType in Enum.GetValues<GLCMFeatureType>())
            {
                double result = glcmFeatures.Calculate(featureType);
                Assert.True(expected.ContainsKey(featureType));
                Assert.InRange(result - expected[featureType], -1e-6, 1e-6);
            }
        }
    }
}
