using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using Radiomics.Net;
using Radiomics.Net.ImageProcess;

namespace Radiomics.Net.Tests;

internal static class TestDataFactory
{
    public static (ImagePlus image, ImagePlus mask, ImagePlus discrete, CaculateParams parameters) CreateStandardImageData()
    {
        var image = new ImagePlus(3, 3, 1)
        {
            PixelWidth = 1d,
            PixelHeight = 1d,
            PixelDepth = 1d,
            BitsAllocated = 16
        };

        var values = new double[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        };

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                image.SetXYZ(x, y, 0, values[y * image.Width + x]);
            }
        }

        var mask = new ImagePlus(image.Width, image.Height, image.Slice)
        {
            PixelWidth = image.PixelWidth,
            PixelHeight = image.PixelHeight,
            PixelDepth = image.PixelDepth,
            BitsAllocated = 16
        };

        for (int y = 0; y < mask.Height; y++)
        {
            for (int x = 0; x < mask.Width; x++)
            {
                mask.SetXYZ(x, y, 0, 1d);
            }
        }

        var discrete = new ImagePlus(image.Width, image.Height, image.Slice)
        {
            PixelWidth = image.PixelWidth,
            PixelHeight = image.PixelHeight,
            PixelDepth = image.PixelDepth,
            BitsAllocated = image.BitsAllocated
        };

        for (int y = 0; y < discrete.Height; y++)
        {
            for (int x = 0; x < discrete.Width; x++)
            {
                discrete.SetXYZ(x, y, 0, values[y * discrete.Width + x]);
            }
        }

        var parameters = new CaculateParams
        {
            Label = 1,
            UseFixedBinNumber = true,
            NBins = 9,
            DensityShift = 0,
            NormalizeScale = 1,
            Force2D = true,
            Interpolation2D = 0,
            MaskPartialVolumeThreshold = 0.5,
            GLCMWeightingMethod = "no_weighting"
        };

        return (image, mask, discrete, parameters);
    }

    public static ImagePlus CreateMaskFor(ImagePlus source, double label = 1)
    {
        var mask = new ImagePlus(source.Width, source.Height, source.Slice)
        {
            PixelWidth = source.PixelWidth,
            PixelHeight = source.PixelHeight,
            PixelDepth = source.PixelDepth,
            BitsAllocated = source.BitsAllocated
        };

        for (int z = 0; z < source.Slice; z++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    mask.SetXYZ(x, y, z, label);
                }
            }
        }

        return mask;
    }

    public static (ImagePlus image, ImagePlus mask) CreateResampleSource()
    {
        var image = new ImagePlus(2, 2, 1)
        {
            PixelWidth = 2d,
            PixelHeight = 2d,
            PixelDepth = 1d,
            BitsAllocated = 16
        };

        var values = new double[] { 1, 2, 3, 4 };
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                image.SetXYZ(x, y, 0, values[y * image.Width + x]);
            }
        }

        var mask = CreateMaskFor(image);
        return (image, mask);
    }

    public static List<DicomDataset> CreateFeatureCalculatorDicoms(ImagePlus image, ImagePlus mask, CaculateParams parameters, ImagePlus? discrete = null)
    {
        var dicomImage = CreateDicomDataset(image);
        var dicomMask = CreateDicomDataset(mask);

        var preprocessSteps = new List<string> { "Normalize", "Resample", "RangeFilter" };
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.Preprocess, preprocessSteps.ToArray());
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.NormalizeScale, parameters.NormalizeScale);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.ResamplingFactorXYZ, new[] { 1d, 1d, 1d });
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.RangeMax, 100d);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.RangeMin, -100d);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.UseFixedBins, parameters.UseFixedBinNumber ? 1 : 0);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.NBins, parameters.NBins);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.EnableFirstOrder, 1);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.EnableGLCM, 1);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.Interpolation2D, parameters.Interpolation2D);
        PrivateDicomTag.AddOrUpdate(dicomImage, PrivateDicomTag.MaskPartialVolumeThreshold, parameters.MaskPartialVolumeThreshold);

        return new List<DicomDataset> { dicomImage, dicomMask };
    }

    private static DicomDataset CreateDicomDataset(ImagePlus source)
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.StudyInstanceUID, DicomUID.Generate() },
            { DicomTag.SeriesInstanceUID, DicomUID.Generate() },
            { DicomTag.Modality, "OT" },
            { DicomTag.PatientID, "TEST" },
            { DicomTag.Rows, (ushort)source.Height },
            { DicomTag.Columns, (ushort)source.Width },
            { DicomTag.PixelSpacing, new[] { source.PixelWidth, source.PixelHeight } },
            { DicomTag.SliceThickness, source.PixelDepth },
            { DicomTag.BitsAllocated, (ushort)source.BitsAllocated },
            { DicomTag.BitsStored, (ushort)source.BitsAllocated },
            { DicomTag.HighBit, (ushort)(source.BitsAllocated - 1) },
            { DicomTag.PhotometricInterpretation, PhotometricInterpretation.Monochrome2.Value },
            { DicomTag.SamplesPerPixel, (ushort)1 },
            { DicomTag.PixelRepresentation, (ushort)0 }
        };

        var pixelData = DicomPixelData.Create(dataset, true);
        pixelData.SamplesPerPixel = 1;
        pixelData.PlanarConfiguration = 0;
        pixelData.BitsStored = (ushort)source.BitsAllocated;
        pixelData.BitsAllocated = (ushort)source.BitsAllocated;
        pixelData.HighBit = (ushort)(source.BitsAllocated - 1);
        pixelData.PixelRepresentation = PixelRepresentation.Unsigned;
        pixelData.Width = source.Width;
        pixelData.Height = source.Height;

        var frame = new ushort[source.Width * source.Height];
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                frame[y * source.Width + x] = (ushort)Math.Round(source.GetXYZ(x, y, 0));
            }
        }

        var bytes = new byte[frame.Length * sizeof(ushort)];
        Buffer.BlockCopy(frame, 0, bytes, 0, bytes.Length);
        pixelData.AddFrame(new MemoryByteBuffer(bytes));

        return dataset;
    }
}
