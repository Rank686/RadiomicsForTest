using System;
using System.Collections.Generic;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

internal static class TestImageFactory
{
    public static ImagePlus CreateImage(double[,] data, double pixelSpacing = 1.0, double pixelDepth = 1.0)
    {
        return CreateImage(ToThreeDimensional(data), pixelSpacing, pixelDepth);
    }

    public static ImagePlus CreateImage(double[,,] data, double pixelSpacing = 1.0, double pixelDepth = 1.0)
    {
        var slices = data.GetLength(0);
        var height = data.GetLength(1);
        var width = data.GetLength(2);
        var image = new ImagePlus(width, height, slices)
        {
            PixelWidth = pixelSpacing,
            PixelHeight = pixelSpacing,
            PixelDepth = pixelDepth,
            BitsAllocated = 64
        };

        for (var z = 0; z < slices; z++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    image.SetXYZ(x, y, z, data[z, y, x]);
                }
            }
        }

        return image;
    }

    public static ImagePlus CreateMaskFromData(double[,] data, double pixelSpacing = 1.0, double pixelDepth = 1.0)
    {
        return CreateMaskFromData(ToThreeDimensional(data), pixelSpacing, pixelDepth);
    }

    public static ImagePlus CreateMaskFromData(double[,,] data, double pixelSpacing = 1.0, double pixelDepth = 1.0)
    {
        var slices = data.GetLength(0);
        var height = data.GetLength(1);
        var width = data.GetLength(2);
        var mask = new ImagePlus(width, height, slices)
        {
            PixelWidth = pixelSpacing,
            PixelHeight = pixelSpacing,
            PixelDepth = pixelDepth,
            BitsAllocated = 16
        };

        for (var z = 0; z < slices; z++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    mask.SetXYZ(x, y, z, data[z, y, x]);
                }
            }
        }

        return mask;
    }

    public static ImagePlus CreateFilledMask(int width, int height, int slices, double value = 1.0, double pixelSpacing = 1.0, double pixelDepth = 1.0)
    {
        var mask = new ImagePlus(width, height, slices)
        {
            PixelWidth = pixelSpacing,
            PixelHeight = pixelSpacing,
            PixelDepth = pixelDepth,
            BitsAllocated = 16
        };

        for (var z = 0; z < slices; z++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    mask.SetXYZ(x, y, z, value);
                }
            }
        }

        return mask;
    }

    public static double[] GetValues(ImagePlus image)
    {
        var values = new List<double>(image.Width * image.Height * image.Slice);
        for (var z = 0; z < image.Slice; z++)
        {
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    values.Add(image.GetXYZ(x, y, z));
                }
            }
        }

        return values.ToArray();
    }

    private static double[,,] ToThreeDimensional(double[,] data)
    {
        var height = data.GetLength(0);
        var width = data.GetLength(1);
        var result = new double[1, height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                result[0, y, x] = data[y, x];
            }
        }

        return result;
    }
}
