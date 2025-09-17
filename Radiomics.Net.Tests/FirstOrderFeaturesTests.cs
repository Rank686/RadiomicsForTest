using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

public class FirstOrderFeaturesTests
{
    private const double Tolerance = 1e-6;

    public static IEnumerable<object[]> FeatureExpectations()
    {
        var (image, mask, discrete, parameters) = TestDataFactory.CreateStandardImageData();
        var expectations = FirstOrderExpectedCalculator.Compute(image, mask, discrete, parameters);
        foreach (var kvp in expectations)
        {
            yield return new object[] { kvp.Key, kvp.Value };
        }
    }

    [Theory]
    [MemberData(nameof(FeatureExpectations))]
    public void Calculate_ReturnsExpectedValues(FirstOrderFeatureType featureType, double expected)
    {
        var (image, mask, discrete, parameters) = TestDataFactory.CreateStandardImageData();
        var features = new FirstOrderFeatures(image, mask, discrete, parameters);

        var actual = features.Calculate(featureType);

        Assert.Equal(expected, actual, Tolerance);
    }
}
