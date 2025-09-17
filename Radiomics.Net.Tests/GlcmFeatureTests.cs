using System;
using System.Collections.Generic;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

internal static class GlcmFeatureTests
{
    private const double Tolerance = 1e-6;

    public static void GlcmFeaturesShouldComputeSelectedMetrics()
    {
        var (features, _, _) = CreateCheckerboardGlcm();
        const double fraction = 1.0 / 4.0;

        var expectations = new Dictionary<GLCMFeatureType, double>
        {
            [GLCMFeatureType.MaximumProbability] = 0.5,
            [GLCMFeatureType.JointEntropy] = 1.0,
            [GLCMFeatureType.JointEnergy] = 0.5,
            [GLCMFeatureType.JointAverage] = 1.5,
            [GLCMFeatureType.SumSquares] = 0.25,
            [GLCMFeatureType.Contrast] = 0.5,
            [GLCMFeatureType.Correlation] = 0.0,
            [GLCMFeatureType.Autocorrection] = 2.25,
            [GLCMFeatureType.DifferenceAverage] = 0.5,
            [GLCMFeatureType.DifferenceVariance] = 0.0,
            [GLCMFeatureType.DifferenceEntropy] = 0.0,
            [GLCMFeatureType.SumAverage] = 3.0,
            [GLCMFeatureType.InverseDifference] = 0.75,
            [GLCMFeatureType.NormalizedInverseDifference] = (2.0 * (2.0 / 3.0) + 2.0) * fraction,
            [GLCMFeatureType.InverseDifferenceMoment] = 0.75,
            [GLCMFeatureType.NormalizedInverseDifferenceMoment] = (2.0 * 0.8 + 2.0) * fraction,
            [GLCMFeatureType.InverseVariance] = 0.5
        };

        foreach (var (feature, expected) in expectations)
        {
            var actual = features.Calculate(feature);
            TestAssert.AreEqual(expected, actual, Tolerance, $"GLCM feature {feature} mismatch.");
        }
    }

    public static void GlcmFeaturesShouldReturnFiniteValuesForAllFeatures()
    {
        var (features, _, _) = CreateCheckerboardGlcm();
        foreach (GLCMFeatureType feature in Enum.GetValues(typeof(GLCMFeatureType)))
        {
            if (feature == GLCMFeatureType.MCC)
            {
                continue;
            }
            var value = features.Calculate(feature);
            TestAssert.IsFalse(double.IsNaN(value), $"GLCM feature {feature} returned NaN.");
            TestAssert.IsFalse(double.IsInfinity(value), $"GLCM feature {feature} returned infinity.");
        }
    }

    private static (GLCMFeatures Features, CaculateParams Parameters, ImagePlus Mask) CreateCheckerboardGlcm()
    {
        var data = new double[,] { { 1, 2, 1 }, { 2, 1, 2 }, { 1, 2, 1 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            Label = 1,
            UseFixedBinNumber = true,
            NBins = 2,
            GLCMDelta = 1,
            Force2D = true,
            EnableGLCM = true
        };

        var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);
        var features = new GLCMFeatures(image, mask, discretised, parameters);
        return (features, parameters, mask);
    }
}
