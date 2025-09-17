using Radiomics.Net;
using Radiomics.Net.ImageProcess;
using Radiomics.Net.Exceptions;

namespace Radiomics.Net.Tests;

public class ImagePreprocessingTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Normalize_ComputesScaledZScores()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        parameters.NormalizeScale = 1d;

        var normalized = ImagePreprocessing.Normalize(image, mask, parameters);

        var voxels = image.Stack[0];
        var mean = voxels.Average();
        var variance = voxels.Select(v => Math.Pow(v - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var expected = (image.GetXYZ(x, y, 0) - mean) / stdDev;
                Assert.Equal(expected, normalized.GetXYZ(x, y, 0), Tolerance);
            }
        }
    }

    [Fact]
    public void Resample_ExpandsImageUsingNearestNeighbour()
    {
        var (image, mask) = TestDataFactory.CreateResampleSource();
        var parameters = new CaculateParams
        {
            Label = 1,
            Force2D = true,
            Interpolation2D = 0,
            MaskPartialVolumeThreshold = 0.5
        };

        var resampled = ImagePreprocessing.Resample(image, mask, new[] { 1d, 1d, 1d }, parameters);

        Assert.NotNull(resampled[0]);
        Assert.NotNull(resampled[1]);
        Assert.Equal(4, resampled[0]!.Width);
        Assert.Equal(4, resampled[0]!.Height);
        Assert.All(resampled[1]!.Stack[0], value => Assert.True(value is 0 or 1));
        Assert.Equal(image.GetXYZ(0, 0, 0), resampled[0]!.GetXYZ(0, 0, 0));
        Assert.Equal(image.GetXYZ(1, 1, 0), resampled[0]!.GetXYZ(resampled[0]!.Width - 1, resampled[0]!.Height - 1, 0));
    }

    [Fact]
    public void Crop_ReturnsBoundingBoxOfRoi()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        for (int y = 0; y < mask.Height; y++)
        {
            for (int x = 0; x < mask.Width; x++)
            {
                mask.SetXYZ(x, y, 0, (x >= 1 && y >= 1) ? 1d : 0d);
            }
        }

        var cropped = ImagePreprocessing.Crop(image, mask, parameters);

        Assert.Equal(2, cropped[0]!.Width);
        Assert.Equal(2, cropped[0]!.Height);
        Assert.Equal(2, cropped[1]!.Width);
        Assert.Equal(2, cropped[1]!.Height);
        Assert.Equal(5, cropped[0]!.GetXYZ(0, 0, 0));
        Assert.Equal(9, cropped[0]!.GetXYZ(1, 1, 0));
    }

    [Fact]
    public void OutlierFiltering_RemovesValuesOutsideStandardDeviationRange()
    {
        var image = new ImagePlus(3, 1, 1);
        image.PixelWidth = image.PixelHeight = image.PixelDepth = 1d;
        image.BitsAllocated = 16;
        double[] values = { 10, 12, 100 };
        for (int x = 0; x < values.Length; x++)
        {
            image.SetXYZ(x, 0, 0, values[x]);
        }
        var mask = TestDataFactory.CreateMaskFor(image);
        var parameters = new CaculateParams
        {
            Label = 1,
            RangeMin = 1,
            RangeMax = 1
        };

        var filteredMask = ImagePreprocessing.OutlierFiltering(image, mask, 1, parameters);

        Assert.Equal(1, filteredMask.GetXYZ(0, 0, 0));
        Assert.Equal(1, filteredMask.GetXYZ(1, 0, 0));
        Assert.Equal(0, filteredMask.GetXYZ(2, 0, 0));
    }

    [Fact]
    public void RangeFiltering_FiltersOutsideAbsoluteRange()
    {
        var image = new ImagePlus(2, 2, 1);
        image.PixelWidth = image.PixelHeight = image.PixelDepth = 1d;
        image.BitsAllocated = 16;
        double[] values = { 1, 2, 3, 4 };
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                image.SetXYZ(x, y, 0, values[y * 2 + x]);
            }
        }
        var mask = TestDataFactory.CreateMaskFor(image);

        var filtered = ImagePreprocessing.RangeFiltering(image, mask, 1, 3, 2);

        Assert.Equal(0, filtered.GetXYZ(0, 0, 0));
        Assert.Equal(1, filtered.GetXYZ(1, 0, 0));
        Assert.Equal(1, filtered.GetXYZ(0, 1, 0));
        Assert.Equal(0, filtered.GetXYZ(1, 1, 0));
    }

    [Fact]
    public void RangeFiltering_InvalidParameters_Throws()
    {
        var (image, mask, _, _) = TestDataFactory.CreateStandardImageData();
        Assert.Throws<CustomException>(() => ImagePreprocessing.RangeFiltering(image, mask, 1, 0, 5));
    }

    [Fact]
    public void PreprocessDiscretise_WithFixedBinNumber_ReturnsExpectedBins()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        parameters.UseFixedBinNumber = true;
        parameters.NBins = 4;

        var discrete = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);

        var voxels = Utils.GetVoxels(discrete, mask, 1);
        Assert.All(voxels, v => Assert.InRange(v, 1, parameters.NBins));
    }

    [Fact]
    public void PreprocessDiscretise_WithBinWidth_UpdatesBinCount()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        parameters.UseFixedBinNumber = false;
        parameters.BinWidth = 2;

        var discrete = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);

        var voxels = Utils.GetVoxels(discrete, mask, 1);
        Assert.All(voxels, v => Assert.True(v >= 1));
        Assert.Equal(Utils.GetNumOfBinsByMax(discrete, mask, 1), parameters.NBins);
    }

    [Fact]
    public void CreateMask_FillsAllVoxelsWithLabel()
    {
        var (image, _, _, _) = TestDataFactory.CreateStandardImageData();
        var mask = ImagePreprocessing.CreateMask(image);
        Assert.All(mask.Stack[0], value => Assert.Equal(1d, value));
    }

    [Fact]
    public void FwtTransform_WithHaarFilterPreservesImage()
    {
        var (image, mask, _, parameters) = TestDataFactory.CreateStandardImageData();
        parameters.WaveFilterName = "haar";

        var filtered = ImagePreprocessing.FwtTransform(image, parameters);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Assert.Equal(image.GetXYZ(x, y, 0), filtered.GetXYZ(x, y, 0), Tolerance);
            }
        }
    }
}
