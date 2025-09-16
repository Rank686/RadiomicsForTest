// See https://aka.ms/new-console-template for more information
using FellowOakDicom;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Radiomics.Net;
using System.Numerics;

namespace Radiomics.Net
{
    public static class Radiomics
    {
        public static void Main(String[] args)
        {
            // 创建一个新的DICOM标签
            var intsTag = new DicomTag(0x0015, 0x0001, "IntsTag");
            

            var doublesTag = new DicomTag(0x0015,0x0002, "doublesTag");

            // 创建一个DICOM文件
            var file = new DicomFile();
            var dataset = file.Dataset;

            // 设置自定义标签的值
            PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.BoxSizes, "1\\2");


            int[] r = dataset.GetValues<int>(PrivateDicomTag.BoxSizes);
            //dataset.AddOrUpdate()

            PrivateDicomTag.AddOrUpdate(dataset, PrivateDicomTag.ResamplingFactorXYZ, new double[] { 0.12, 0.24, 0.48 });
            dataset.Add(DicomVR.DS, doublesTag, new double[] { 0.12, 0.24 });

            int[] ints = dataset.GetValues<int>(intsTag);

            file = DicomFile.Open(@"F:\study\Radiomics\RadiomicsJ\src\test\resources\data_sets-master\ibsi_1_ct_radiomics_phantom\dicom\image\DCM_IMG_00000.dcm");
            dataset = file.Dataset;
            //String val = dataset<String>(DicomTag.PixelSpacing);

            foreach (DicomDataset roiDataSet in dataset.GetSequence(DicomTag.StructureSetROISequence)) { 
                //roiDataSet.AddOrUpdate
            }

            var matrix = Matrix<double>.Build.Dense(2, 2, new double[] { 1.0, 2.0, 3.0, 4.0 });

            var evd = matrix.Evd();
            MathNet.Numerics.LinearAlgebra.Vector<double> eigenValues = evd.EigenValues.Map(c=>c.Real);
        }
    }
}

