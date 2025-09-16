using System;
using System.Collections.Generic;
using System.Linq;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using Radiomics.Net;
using Radiomics.Net.Exceptions;
using Xunit;

namespace Radiomics.Net.Tests
{
    public class FeatureCalculatorTests
    {
        [Fact]
        public void Calculate_ThrowsCustomException_WhenDicomListIsEmpty()
        {
            var exception = Assert.Throws<CustomException>(() => FeatureCalculator.Calclate(new List<DicomDataset>()));

            Assert.Equal((int)Errors.ParamsError, exception.Result.ErrorCode);
        }

        [Fact]
        public void Calculate_ThrowsCustomException_WhenMaskHasNoRoi()
        {
            var image = TestDicomFactory.CreateImageDataset(new ushort[,] { { 1, 2 }, { 3, 4 } });
            var mask = TestDicomFactory.CreateMaskDataset(new ushort[,] { { 0, 0 }, { 0, 0 } });

            var exception = Assert.Throws<CustomException>(() => FeatureCalculator.Calclate(new List<DicomDataset> { image, mask }));

            Assert.Equal((int)Errors.ROIDataError, exception.Result.ErrorCode);
        }

        [Fact]
        public void Calculate_WritesFirstOrderFeatures_WhenFirstOrderEnabled()
        {
            var image = TestDicomFactory.CreateImageDataset(new ushort[,] { { 1, 2 }, { 3, 4 } });
            TestDicomFactory.ConfigureFeatureFlags(image, enableFirstOrder: true, enableGlcm: false, useFixedBins: true, nBins: 4);
            var mask = TestDicomFactory.CreateMaskDataset(new ushort[,] { { 1, 1 }, { 1, 1 } });

            var result = FeatureCalculator.Calclate(new List<DicomDataset> { image, mask });

            Assert.True(result.IsSuccess);
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.Mean, out double mean));
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.Minimum, out double minimum));
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.Maximum, out double maximum));
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.Range, out double range));
            Assert.Equal(2.5, mean, 3);
            Assert.Equal(1.0, minimum, 3);
            Assert.Equal(4.0, maximum, 3);
            Assert.Equal(3.0, range, 3);
            Assert.False(image.Contains(PrivateDicomTag.JointEnergy));
        }

        [Fact]
        public void Calculate_WritesGlcmFeatures_WhenGlcmEnabled()
        {
            var image = TestDicomFactory.CreateImageDataset(new ushort[,] { { 1, 2 }, { 3, 4 } });
            TestDicomFactory.ConfigureFeatureFlags(image, enableFirstOrder: false, enableGlcm: true, useFixedBins: true, nBins: 4);
            var mask = TestDicomFactory.CreateMaskDataset(new ushort[,] { { 1, 1 }, { 1, 1 } });

            var result = FeatureCalculator.Calclate(new List<DicomDataset> { image, mask });

            Assert.True(result.IsSuccess);
            Assert.False(image.Contains(PrivateDicomTag.Mean));
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.JointEnergy, out double jointEnergy));
            Assert.True(image.TryGetSingleValue(PrivateDicomTag.Contrast, out double contrast));
            Assert.True(jointEnergy >= 0);
            Assert.True(contrast >= 0);
        }
    }

    internal static class TestDicomFactory
    {
        public static DicomDataset CreateImageDataset(ushort[,] pixelValues)
        {
            return CreateDataset(pixelValues);
        }

        public static DicomDataset CreateMaskDataset(ushort[,] pixelValues)
        {
            return CreateDataset(pixelValues);
        }

        public static void ConfigureFeatureFlags(DicomDataset dataset, bool enableFirstOrder, bool enableGlcm, bool useFixedBins, int nBins)
        {
            PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.EnableFirstOrder, enableFirstOrder ? 1 : 0);
            PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.EnableGLCM, enableGlcm ? 1 : 0);
            PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.UseFixedBins, useFixedBins ? 1 : 0);
            if (useFixedBins)
            {
                PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.NBins, nBins);
            }
        }

        private static DicomDataset CreateDataset(ushort[,] values)
        {
            int height = values.GetLength(0);
            int width = values.GetLength(1);
            var dataset = new DicomDataset(DicomTransferSyntax.ExplicitVRLittleEndian)
            {
                { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
                { DicomTag.SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID() },
                { DicomTag.PatientID, "TEST" },
                { DicomTag.Modality, "OT" },
                { DicomTag.Rows, (ushort)height },
                { DicomTag.Columns, (ushort)width },
                { DicomTag.SamplesPerPixel, (ushort)1 },
                { DicomTag.PhotometricInterpretation, PhotometricInterpretation.Monochrome2.Value },
                { DicomTag.BitsAllocated, (ushort)16 },
                { DicomTag.BitsStored, (ushort)16 },
                { DicomTag.HighBit, (ushort)15 },
                { DicomTag.PixelRepresentation, (ushort)0 },
                { DicomTag.PixelSpacing, new double[] { 1.0, 1.0 } },
                { DicomTag.SliceThickness, 1.0 }
            };

            var pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsAllocated = 16;
            pixelData.BitsStored = 16;
            pixelData.HighBit = 15;
            pixelData.SamplesPerPixel = 1;
            pixelData.PixelRepresentation = PixelRepresentation.Unsigned;

            var frame = new byte[width * height * 2];
            var buffer = new ushort[width * height];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[index++] = values[y, x];
                }
            }

            Buffer.BlockCopy(buffer, 0, frame, 0, frame.Length);
            pixelData.AddFrame(new MemoryByteBuffer(frame));

            return dataset;
        }
    }
}
