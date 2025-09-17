using Radiomics.Net.Features;

namespace Radiomics.Net.Tests;

public class GlcmFeaturesTests
{
    private const double Tolerance = 1e-6;

    public static IEnumerable<object[]> FeatureExpectations()
    {
        var (image, mask, discrete, parameters) = TestDataFactory.CreateStandardImageData();
        var glcmFeatures = new GLCMFeatures(image, mask, discrete, parameters);
        var expectations = GlcmExpectedCalculator.Compute(glcmFeatures);
        foreach (var kvp in expectations)
        {
            yield return new object[] { kvp.Key, kvp.Value };
        }
    }

    [Theory]
    [MemberData(nameof(FeatureExpectations))]
    public void Calculate_ReturnsExpectedValues(GLCMFeatureType featureType, double expected)
    {
        var (image, mask, discrete, parameters) = TestDataFactory.CreateStandardImageData();
        var features = new GLCMFeatures(image, mask, discrete, parameters);

        var actual = features.Calculate(featureType);

        if (double.IsNaN(expected))
        {
            Assert.True(double.IsNaN(actual));
        }
        else
        {
            Assert.Equal(expected, actual, Tolerance);
        }
    }
}
