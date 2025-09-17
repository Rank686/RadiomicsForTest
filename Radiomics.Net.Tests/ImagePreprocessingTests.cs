using System;
using System.Linq;
using Radiomics.Net;
using Radiomics.Net.ImageProcess;
using Xunit;

namespace Radiomics.Net.Tests
{
    public class ImagePreprocessingTests
    {
        private static (ImagePlus image, ImagePlus mask) CreateImageAndMask(double[,] values, int label = 1)
        {
            int height = values.GetLength(0);
            int width = values.GetLength(1);
            var image = new ImagePlus(width, height, 1)
            {
                PixelWidth = 1,
                PixelHeight = 1,
                PixelDepth = 1,
                BitsAllocated = 16
            };
            var mask = new ImagePlus(width, height, 1)
            {
                PixelWidth = 1,
                PixelHeight = 1,
                PixelDepth = 1,
                BitsAllocated = 16
            };
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image.SetXYZ(x, y, 0, values[y, x]);
                    mask.SetXYZ(x, y, 0, label);
                }
            }
            return (image, mask);
        }

        private static double[] GetMaskVoxels(ImagePlus image, ImagePlus mask, int label = 1)
        {
            return Utils.GetVoxels(image, mask, label);
        }

        [Fact]
        public void Normalize_ScalesValuesToZeroMeanAndConfiguredStd()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 2 }, { 3, 4 } });
            var parameters = new CaculateParams
            {
                Label = 1,
                NormalizeScale = 2.0
            };

            var normalized = ImagePreprocessing.Normalize(image, mask, parameters);
            var voxels = GetMaskVoxels(normalized, mask);
            double mean = voxels.Average();
            double variance = voxels.Select(v => Math.Pow(v - mean, 2)).Average();

            Assert.InRange(mean, -1e-10, 1e-10);
            Assert.InRange(Math.Sqrt(variance), 2.0 - 1e-10, 2.0 + 1e-10);
        }

        [Fact]
        public void Resample_ProducesExpectedDimensionsAndMetadata()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 2 }, { 3, 4 } });
            image.PixelWidth = 2;
            image.PixelHeight = 2;
            mask.PixelWidth = 2;
            mask.PixelHeight = 2;
            var parameters = new CaculateParams
            {
                Label = 1,
                Force2D = true,
                Interpolation2D = 0,
                ResamplingFactorXYZ = new[] { 1.0, 1.0, 1.0 }
            };

            var resampled = ImagePreprocessing.Resample(image, mask, parameters.ResamplingFactorXYZ, parameters);
            Assert.NotNull(resampled[0]);
            Assert.NotNull(resampled[1]);
            Assert.Equal(4, resampled[0].Width);
            Assert.Equal(4, resampled[0].Height);
            Assert.Equal(1.0, resampled[0].PixelWidth);
            Assert.Equal(1.0, resampled[0].PixelHeight);
            Assert.Equal(4, resampled[1].Width);
            Assert.All(resampled[1].Stack[0], v => Assert.True(v == 0 || Math.Abs(v - parameters.Label) < 1e-10));
        }

        [Fact]
        public void Crop_ReturnsBoundingBoxAroundLabel()
        {
            var (image, mask) = CreateImageAndMask(new double[,] {
                { 0, 1, 2, 3 },
                { 4, 5, 6, 7 },
                { 8, 9,10,11 },
                {12,13,14,15 }
            });
            // restrict ROI to center 2x2 region
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    mask.SetXYZ(x, y, 0, (x >= 1 && x <= 2 && y >= 1 && y <= 2) ? 1 : 0);
                }
            }
            var parameters = new CaculateParams { Label = 1 };

            var cropped = ImagePreprocessing.Crop(image, mask, parameters);
            Assert.Equal(2, cropped[0].Width);
            Assert.Equal(2, cropped[0].Height);
            double[] expected = { 5, 6, 9, 10 };
            Assert.Equal(expected, cropped[0].Stack[0]);
        }

        [Fact]
        public void OutlierFiltering_RemovesValuesOutsideConfiguredStdRange()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 10, 10 }, { 10, 100 } });
            var parameters = new CaculateParams
            {
                Label = 1,
                RangeMin = 1,
                RangeMax = 1
            };

            var filtered = ImagePreprocessing.OutlierFiltering(image, mask, (int)parameters.Label, parameters);
            Assert.Equal(0, filtered.GetXYZ(1, 1, 0));
            Assert.Equal(1, filtered.GetXYZ(0, 0, 0));
        }

        [Fact]
        public void RangeFiltering_RemovesValuesOutsideRange()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 5 }, { 3, 7 } });
            var filtered = ImagePreprocessing.RangeFiltering(image, mask, 1, 5, 2);
            Assert.Equal(0, filtered.GetXYZ(1, 1, 0));
            Assert.Equal(1, filtered.GetXYZ(0, 0, 0));
            Assert.Equal(1, filtered.GetXYZ(1, 0, 0));
        }

        [Fact]
        public void PreprocessDiscretise_UsesFixedBinNumber()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 2 }, { 3, 4 } });
            var parameters = new CaculateParams
            {
                Label = 1,
                UseFixedBinNumber = true,
                NBins = 2
            };

            var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, (int)parameters.Label, parameters);
            var voxels = GetMaskVoxels(discretised, mask);
            Assert.Equal(new[] { 1d, 1d, 2d, 2d }, voxels);
        }

        [Fact]
        public void PreprocessDiscretise_UsesBinWidth()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 2 }, { 3, 4 } });
            var parameters = new CaculateParams
            {
                Label = 1,
                UseFixedBinNumber = false,
                BinWidth = 1
            };

            var discretised = ImagePreprocessing.PreprocessDiscretise(image, mask, (int)parameters.Label, parameters);
            var voxels = GetMaskVoxels(discretised, mask);
            Assert.Equal(new[] { 1d, 2d, 3d, 4d }, voxels);
            Assert.Equal(4, parameters.NBins);
        }

        [Fact]
        public void CreateMask_SetsAllVoxelsToOne()
        {
            var (image, _) = CreateImageAndMask(new double[,] { { 0, 2 }, { 3, 4 } });
            var mask = ImagePreprocessing.CreateMask(image);
            Assert.All(mask.Stack[0], v => Assert.Equal(1d, v));
        }

        [Fact]
        public void FwtTransform_PreservesSignalAfterForwardAndInverse()
        {
            var (image, mask) = CreateImageAndMask(new double[,] { { 1, 2 }, { 3, 4 } });
            var parameters = new CaculateParams
            {
                Label = 1,
                WaveFilterName = "haar"
            };

            var transformed = ImagePreprocessing.FwtTransform(image, parameters);
            var originalVoxels = GetMaskVoxels(image, mask);
            var transformedVoxels = GetMaskVoxels(transformed, mask);
            for (int i = 0; i < originalVoxels.Length; i++)
            {
                Assert.InRange(transformedVoxels[i] - originalVoxels[i], -1e-6, 1e-6);
            }
        }
    }
}
