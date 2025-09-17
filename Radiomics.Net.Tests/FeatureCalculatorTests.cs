using System;
using System.Collections.Generic;
using FellowOakDicom;
using FellowOakDicom.IO.Buffer;
using FellowOakDicom.Imaging;
using Radiomics.Net.Exceptions;
using Xunit;

namespace Radiomics.Net.Tests
{
    public class FeatureCalculatorTests
    {
        private static DicomDataset CreateImageDataset(ushort[] pixelValues, int width, int height)
        {
            var dataset = new DicomDataset(DicomTransferSyntax.ExplicitVRLittleEndian)
            {
                { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
                { DicomTag.SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID() },
                { DicomTag.PatientID, "TEST" },
                { DicomTag.Rows, (ushort)height },
                { DicomTag.Columns, (ushort)width },
                { DicomTag.BitsAllocated, (ushort)16 },
                { DicomTag.BitsStored, (ushort)16 },
                { DicomTag.HighBit, (ushort)15 },
                { DicomTag.PixelRepresentation, (ushort)0 },
                { DicomTag.SamplesPerPixel, (ushort)1 },
                { DicomTag.PhotometricInterpretation, PhotometricInterpretation.Monochrome2.Value },
                { DicomTag.PixelSpacing, new double[] { 1.0, 1.0 } },
                { DicomTag.SliceThickness, 1.0 }
            };

            var pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsStored = 16;
            pixelData.BitsAllocated = 16;
            pixelData.HighBit = 15;
            pixelData.PixelRepresentation = PixelRepresentation.Unsigned;
            pixelData.SamplesPerPixel = 1;
            pixelData.Width = width;
            pixelData.Height = height;
            var buffer = new byte[pixelValues.Length * sizeof(ushort)];
            System.Buffer.BlockCopy(pixelValues, 0, buffer, 0, buffer.Length);
            pixelData.AddFrame(new MemoryByteBuffer(buffer));
            return dataset;
        }

        [Fact]
        public void Calculate_CompletesFullPipelineAndPopulatesFeatureTags()
        {
            ushort[] pixelValues =
            {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            };
            var imageDataset = CreateImageDataset(pixelValues, 3, 3);
            var maskDataset = CreateImageDataset(new ushort[]
            {
                1, 1, 1,
                1, 1, 1,
                1, 1, 1
            }, 3, 3);

            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.Preprocess, new[] { "Normalize", "Resample", "RangeFilter" });
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.NormalizeScale, 1.0);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.ResamplingFactorXYZ, new[] { 1.0, 1.0, 1.0 });
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.RangeMax, 9.0);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.RangeMin, 1.0);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.UseFixedBins, 1);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.NBins, 2);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.EnableGLCM, 1);
            PrivateDicomTag.AddOrUpdate(imageDataset, PrivateDicomTag.EnableFirstOrder, 1);

            var datasets = new List<DicomDataset> { imageDataset, maskDataset };
            var result = FeatureCalculator.Calclate(datasets);

            Assert.True(result.IsSuccess);
            Assert.Equal((int)Errors.OK, result.ErrorCode);

            Assert.True(imageDataset.TryGetValue<double>(PrivateDicomTag.Mean, 0, out var mean));
            Assert.False(double.IsNaN(mean));
            Assert.True(imageDataset.TryGetValue<double>(PrivateDicomTag.MaximumProbability, 0, out var glcmValue));
            Assert.False(double.IsNaN(glcmValue));
        }
    }
}
