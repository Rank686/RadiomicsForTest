using Radiomics.Net.Exceptions;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

public class FeatureCalculatorTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Calculate_ComputesEnabledFeaturesAndStoresInDicom()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        parameters.Preprocess = new[] { "Normalize", "Resample", "RangeFilter" };
        parameters.NormalizeScale = 1d;
        parameters.ResamplingFactorXYZ = new[] { 1d, 1d, 1d };
        parameters.RangeMax = 100d;
        parameters.RangeMin = -100d;
        parameters.UseFixedBinNumber = true;
        parameters.NBins = 9;
        parameters.Interpolation2D = 0;
        parameters.MaskPartialVolumeThreshold = 0.5;
        parameters.Force2D = true;

        var datasets = TestDataFactory.CreateFeatureCalculatorDicoms(image, mask, parameters);

        var processed = PrepareForExpectations(image, mask, parameters);
        var firstOrderExpectations = FirstOrderExpectedCalculator.Compute(processed.Image, processed.Mask, processed.Discrete, parameters);
        var glcmFeatures = new GLCMFeatures(processed.Image, processed.Mask, processed.Discrete, parameters);
        var glcmExpectations = GlcmExpectedCalculator.Compute(glcmFeatures);

        var result = FeatureCalculator.Calclate(datasets);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)Errors.OK, result.ErrorCode);

        var dataset = datasets[0];
        foreach (var kvp in firstOrderExpectations)
        {
            var tag = PrivateDicomTag.GetPrivateDicomTagByName(kvp.Key.ToString());
            Assert.True(dataset.TryGetValue<double>(tag, 0, out var actual));
            Assert.Equal(kvp.Value, actual, Tolerance);
        }

        foreach (var kvp in glcmExpectations)
        {
            var tag = PrivateDicomTag.GetPrivateDicomTagByName(kvp.Key.ToString());
            Assert.True(dataset.TryGetValue<double>(tag, 0, out var actual));
            if (double.IsNaN(kvp.Value))
            {
                Assert.True(double.IsNaN(actual));
            }
            else
            {
                Assert.Equal(kvp.Value, actual, Tolerance);
            }
        }
    }

    private static (ImagePlus Image, ImagePlus Mask, ImagePlus Discrete) PrepareForExpectations(ImagePlus sourceImage, ImagePlus sourceMask, CaculateParams parameters)
    {
        ImagePlus image = sourceImage;
        ImagePlus mask = sourceMask;

        if (parameters.Preprocess.Contains("Normalize"))
        {
            image = ImagePreprocessing.Normalize(image, mask, parameters);
        }

        if (parameters.Preprocess.Contains("Resample") && parameters.ResamplingFactorXYZ != null)
        {
            var resampled = ImagePreprocessing.Resample(image, mask, parameters.ResamplingFactorXYZ, parameters);
            image = resampled[0] ?? image;
            mask = resampled[1] ?? mask;
        }

        if (parameters.Preprocess.Contains("RangeFilter"))
        {
            mask = ImagePreprocessing.RangeFiltering(image, mask, (int)parameters.Label, parameters.RangeMax, parameters.RangeMin);
        }

        if (!string.IsNullOrWhiteSpace(parameters.WaveFilterName))
        {
            image = ImagePreprocessing.FwtTransform(image, parameters);
        }

        var cropped = ImagePreprocessing.Crop(image, mask, parameters);
        image = cropped[0] ?? image;
        mask = cropped[1] ?? mask;

        ImagePlus discrete;
        if (parameters.UseFixedBinNumber)
        {
            discrete = Utils.Discrete(image, mask, (int)parameters.Label, parameters.NBins);
        }
        else
        {
            discrete = Utils.DiscreteByBinWidth(image, mask, (int)parameters.Label, parameters);
            parameters.NBins = Utils.GetNumOfBinsByMax(discrete, mask, (int)parameters.Label);
        }

        return (image, mask, discrete);
    }
}
