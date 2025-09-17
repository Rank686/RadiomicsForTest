using System;
using System.Linq;
using Radiomics.Net.Exceptions;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

internal static class ImagePreprocessingTests
{
    private const double Tolerance = 1e-6;

    public static void NormalizeShouldStandardizeData()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            Label = 1,
            NormalizeScale = 1.0
        };

        var normalized = ImagePreprocessing.Normalize(image, mask, parameters);
        var voxels = Utils.GetVoxels(normalized, mask, (int)parameters.Label);
        TestAssert.AreEqual(0.0, voxels.Average(), Tolerance, "Normalized voxels should have zero mean.");
        TestAssert.AreEqual(parameters.NormalizeScale, ComputeStandardDeviation(voxels), Tolerance, "Standard deviation should match scaling.");

        var originalVoxels = TestImageFactory.GetValues(image);
        var originalMean = originalVoxels.Average();
        var originalStd = ComputeStandardDeviation(originalVoxels);
        var expectedFirst = (data[0, 0] - originalMean) / originalStd * parameters.NormalizeScale;
        TestAssert.AreEqual(expectedFirst, normalized.GetXYZ(0, 0, 0), Tolerance, "First voxel should be normalized using z-score.");
    }

    public static void ResampleShouldChangeDimensionsUsingNearestNeighbor()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data, pixelSpacing: 2.0, pixelDepth: 5.0);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1, pixelSpacing: 2.0, pixelDepth: 5.0);

        var parameters = new CaculateParams
        {
            Label = 1,
            Interpolation2D = 0,
            Force2D = true,
            MaskPartialVolumeThreshold = 0.5,
            ResamplingFactorXYZ = new[] { 1.0, 1.0, 1.0 }
        };

        var result = ImagePreprocessing.Resample(image, mask, parameters.ResamplingFactorXYZ!, parameters);
        var resampled = result[0];
        var resampledMask = result[1];

        TestAssert.AreEqual(4, resampled.Width, "Width should scale according to voxel spacing.");
        TestAssert.AreEqual(4, resampled.Height, "Height should scale according to voxel spacing.");
        TestAssert.AreEqual(1, resampled.Slice, "Slice count should remain unchanged.");
        TestAssert.AreEqual(1.0, resampled.PixelWidth, Tolerance, "Pixel width should match target spacing.");
        TestAssert.AreEqual(1.0, resampled.PixelHeight, Tolerance, "Pixel height should match target spacing.");
        TestAssert.AreEqual(5.0, resampled.PixelDepth, Tolerance, "Pixel depth should be preserved.");

        TestAssert.AreEqual(1.0, resampled.GetXYZ(0, 0, 0), Tolerance);
        TestAssert.AreEqual(1.0, resampled.GetXYZ(1, 0, 0), Tolerance);
        TestAssert.AreEqual(2.0, resampled.GetXYZ(2, 0, 0), Tolerance);
        TestAssert.AreEqual(4.0, resampled.GetXYZ(3, 3, 0), Tolerance);

        for (var z = 0; z < resampledMask.Slice; z++)
        {
            for (var y = 0; y < resampledMask.Height; y++)
            {
                for (var x = 0; x < resampledMask.Width; x++)
                {
                    TestAssert.AreEqual(1.0, resampledMask.GetXYZ(x, y, z), Tolerance, "Mask voxels should remain inside ROI.");
                }
            }
        }
    }

    public static void CropShouldTrimToMaskBounds()
    {
        var data = new double[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 }, { 13, 14, 15, 16 } };
        var image = TestImageFactory.CreateImage(data);
        var maskData = new double[,] { { 0, 0, 0, 0 }, { 0, 1, 1, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 } };
        var mask = TestImageFactory.CreateMaskFromData(maskData);

        var parameters = new CaculateParams { Label = 1 };
        var result = ImagePreprocessing.Crop(image, mask, parameters);
        var croppedImage = result[0];
        var croppedMask = result[1];

        TestAssert.AreEqual(2, croppedImage.Width);
        TestAssert.AreEqual(2, croppedImage.Height);
        TestAssert.AreEqual(1, croppedImage.Slice);
        TestAssert.AreEqual(6.0, croppedImage.GetXYZ(0, 0, 0), Tolerance);
        TestAssert.AreEqual(7.0, croppedImage.GetXYZ(1, 0, 0), Tolerance);
        TestAssert.AreEqual(10.0, croppedImage.GetXYZ(0, 1, 0), Tolerance);
        TestAssert.AreEqual(11.0, croppedImage.GetXYZ(1, 1, 0), Tolerance);

        for (var z = 0; z < croppedMask.Slice; z++)
        {
            for (var y = 0; y < croppedMask.Height; y++)
            {
                for (var x = 0; x < croppedMask.Width; x++)
                {
                    TestAssert.AreEqual(1.0, croppedMask.GetXYZ(x, y, z), Tolerance);
                }
            }
        }
    }

    public static void RangeFilteringShouldZeroOutValuesOutsideRange()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var filtered = ImagePreprocessing.RangeFiltering(image, mask, 1, 3, 2);

        TestAssert.AreEqual(0.0, filtered.GetXYZ(0, 0, 0), Tolerance);
        TestAssert.AreEqual(1.0, filtered.GetXYZ(1, 0, 0), Tolerance);
        TestAssert.AreEqual(1.0, filtered.GetXYZ(0, 1, 0), Tolerance);
        TestAssert.AreEqual(0.0, filtered.GetXYZ(1, 1, 0), Tolerance);
    }

    public static void OutlierFilteringShouldZeroOutExtremeValues()
    {
        var data = new double[,] { { 1, 1 }, { 1, 10 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            RangeMin = 1,
            RangeMax = 1
        };

        var filtered = ImagePreprocessing.OutlierFiltering(image, mask, 1, parameters);
        TestAssert.AreEqual(1.0, filtered.GetXYZ(0, 0, 0), Tolerance);
        TestAssert.AreEqual(1.0, filtered.GetXYZ(1, 0, 0), Tolerance);
        TestAssert.AreEqual(1.0, filtered.GetXYZ(0, 1, 0), Tolerance);
        TestAssert.AreEqual(0.0, filtered.GetXYZ(1, 1, 0), Tolerance);
    }

    public static void CreateMaskShouldFillRegionWithOnes()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = ImagePreprocessing.CreateMask(image);

        for (var z = 0; z < mask.Slice; z++)
        {
            for (var y = 0; y < mask.Height; y++)
            {
                for (var x = 0; x < mask.Width; x++)
                {
                    TestAssert.AreEqual(1.0, mask.GetXYZ(x, y, z), Tolerance);
                }
            }
        }
    }

    public static void PreprocessDiscretiseShouldRespectFixedBins()
    {
        var data = new double[,] { { 1, 2 }, { 3, 4 } };
        var image = TestImageFactory.CreateImage(data);
        var mask = TestImageFactory.CreateFilledMask(image.Width, image.Height, image.Slice, value: 1);

        var parameters = new CaculateParams
        {
            Label = 1,
            UseFixedBinNumber = true,
            NBins = 4
        };

        var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, 1, parameters);
        TestAssert.AreEqual(1.0, discretised.GetXYZ(0, 0, 0), Tolerance);
        TestAssert.AreEqual(2.0, discretised.GetXYZ(1, 0, 0), Tolerance);
        TestAssert.AreEqual(3.0, discretised.GetXYZ(0, 1, 0), Tolerance);
        TestAssert.AreEqual(4.0, discretised.GetXYZ(1, 1, 0), Tolerance);
    }

    public static void FwtTransformShouldFailForUnknownFilter()
    {
        var data = new double[,] { { 5, 7 }, { 3, 1 } };
        var image = TestImageFactory.CreateImage(data);
        var parameters = new CaculateParams
        {
            WaveFilterName = "missing-filter.flt"
        };

        var threw = false;
        try
        {
            ImagePreprocessing.FwtTransform(image, parameters);
        }
        catch (CustomException ex)
        {
            threw = ex.Message.Contains("滤波器");
        }
        catch (FormatException)
        {
            threw = true;
        }

        TestAssert.IsTrue(threw, "Wavelet transform should report missing filters.");
    }

    private static double ComputeStandardDeviation(double[] values)
    {
        var mean = values.Average();
        var variance = values.Select(v => Math.Pow(v - mean, 2)).Sum() / values.Length;
        return Math.Sqrt(variance);
    }
}
