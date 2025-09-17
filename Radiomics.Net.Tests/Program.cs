using System;

namespace Radiomics.Net.Tests;

internal static class Program
{
    private static int Main()
    {
        var tests = new (string Name, Action Test)[]
        {
            ("Normalize standardizes image", ImagePreprocessingTests.NormalizeShouldStandardizeData),
            ("Resample scales image dimensions", ImagePreprocessingTests.ResampleShouldChangeDimensionsUsingNearestNeighbor),
            ("Crop trims to mask bounds", ImagePreprocessingTests.CropShouldTrimToMaskBounds),
            ("Range filter removes out-of-range voxels", ImagePreprocessingTests.RangeFilteringShouldZeroOutValuesOutsideRange),
            ("Outlier filter removes extreme voxels", ImagePreprocessingTests.OutlierFilteringShouldZeroOutExtremeValues),
            ("CreateMask fills ROI", ImagePreprocessingTests.CreateMaskShouldFillRegionWithOnes),
            ("Discretise respects fixed bins", ImagePreprocessingTests.PreprocessDiscretiseShouldRespectFixedBins),
            ("Wavelet transform reports unknown filters", ImagePreprocessingTests.FwtTransformShouldFailForUnknownFilter),
            ("First order features produce expected statistics", FirstOrderFeatureTests.FirstOrderFeaturesShouldComputeExpectedStatistics),
            ("First order features use histogram bins", FirstOrderFeatureTests.FirstOrderFeaturesShouldUseHistogramWhenBinsProvided),
            ("GLCM features compute selected metrics", GlcmFeatureTests.GlcmFeaturesShouldComputeSelectedMetrics),
            ("GLCM features return finite values", GlcmFeatureTests.GlcmFeaturesShouldReturnFiniteValuesForAllFeatures)
        };

        foreach (var (name, test) in tests)
        {
            TestRunner.Run(name, test);
        }

        return TestRunner.Report();
    }
}
