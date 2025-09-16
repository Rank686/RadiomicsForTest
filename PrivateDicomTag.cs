using FellowOakDicom;
using Radiomics.Net.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net
{
    public class PrivateDicomTag
    {
        //params
        public static readonly DicomTag Interpolation2D = new DicomTag(0x0013, 0x0001, "Interpolation2D");
        public static readonly DicomTag Interpolation3D = new DicomTag(0x0013, 0x0002, "Interpolation3D");
        public static readonly DicomTag MaskPartialVolumeThreshold = new DicomTag(0x0013, 0x0003, "MaskPartialVolumeThreshold");
        public static readonly DicomTag BinWidth = new DicomTag(0x0013, 0x0004, "BinWidth");
        public static readonly DicomTag IVHBinWidth = new DicomTag(0x0013, 0x0005, "IVHBinWidth");
        public static readonly DicomTag NormalizeScale = new DicomTag(0x0013, 0x0007, "NormalizeScale");
        public static readonly DicomTag RangeMax = new DicomTag(0x0013, 0x0008, "RangeMax");
        public static readonly DicomTag RangeMin = new DicomTag(0x0013, 0x0009, "RangeMin");
        public static readonly DicomTag Rlpha = new DicomTag(0x0013, 0x000A, "Rlpha");
        public static readonly DicomTag DeltaGLCM = new DicomTag(0x0013, 0x000B, "DeltaGLCM");
        public static readonly DicomTag DeltaNGTDM = new DicomTag(0x0013, 0x000C, "DeltaNGTDM");
        public static readonly DicomTag DeltaNGLDM = new DicomTag(0x0013, 0x000D, "DeltaNGLDM");
        public static readonly DicomTag BoxSizes = new DicomTag(0x0013, 0x000E, "BoxSizes");
        public static readonly DicomTag ResamplingFactorXYZ = new DicomTag(0x0013, 0x000F, "ResamplingFactorXYZ");
        public static readonly DicomTag Force2D = new DicomTag(0x0013, 0x0010, "Force2D");
        public static readonly DicomTag WaveletFilterName = new DicomTag(0x0013, 0x0011, "WaveletFilterName");
        public static readonly DicomTag EnableGLCM = new DicomTag(0x0013, 0x0012, "EnableGLCM");
        public static readonly DicomTag RangeFilterMode = new DicomTag(0x0013, 0x0013, "RangeFilterMode");
        public static readonly DicomTag UseFixedBins = new DicomTag(0x0013, 0x0014, "UseFixedBins");
        public static readonly DicomTag NBins = new DicomTag(0x0013, 0x0015, "NBins");
        public static readonly DicomTag Preprocess = new DicomTag(0x0013, 0x0016, "Preprocess");
        public static readonly DicomTag EnableFirstOrder = new DicomTag(0x0013, 0x0017, "EnableFirstOrder");

        //GLCM
        public static readonly DicomTag MaximumProbability = new DicomTag(0x0015, 0x0001, GLCMFeatureType.MaximumProbability.ToString());
        public static readonly DicomTag JointAverage = new DicomTag(0x0015, 0x0002, GLCMFeatureType.JointAverage.ToString());
        public static readonly DicomTag SumSquares = new DicomTag(0x0015, 0x0003, GLCMFeatureType.SumSquares.ToString());
        public static readonly DicomTag JointEntropy = new DicomTag(0x0015, 0x0004, GLCMFeatureType.JointEntropy.ToString());
        public static readonly DicomTag DifferenceAverage = new DicomTag(0x0015, 0x0005, GLCMFeatureType.DifferenceAverage.ToString());
        public static readonly DicomTag DifferenceVariance = new DicomTag(0x0015, 0x0006, GLCMFeatureType.DifferenceVariance.ToString());
        public static readonly DicomTag DifferenceEntropy = new DicomTag(0x0015, 0x0007, GLCMFeatureType.DifferenceEntropy.ToString());
        public static readonly DicomTag SumAverage = new DicomTag(0x0015, 0x0008, GLCMFeatureType.SumAverage.ToString());
        public static readonly DicomTag SumVariance = new DicomTag(0x0015, 0x0009, GLCMFeatureType.SumVariance.ToString());
        public static readonly DicomTag SumEntropy = new DicomTag(0x0015, 0x000A, GLCMFeatureType.SumEntropy.ToString());
        public static readonly DicomTag JointEnergy = new DicomTag(0x0015, 0x000B, GLCMFeatureType.JointEnergy.ToString());
        public static readonly DicomTag Contrast = new DicomTag(0x0015, 0x000C, GLCMFeatureType.Contrast.ToString());
        public static readonly DicomTag InverseDifference = new DicomTag(0x0015, 0x000D, GLCMFeatureType.InverseDifference.ToString());
        public static readonly DicomTag NormalizedInverseDifference = new DicomTag(0x0015, 0x000E, GLCMFeatureType.NormalizedInverseDifference.ToString());
        public static readonly DicomTag InverseDifferenceMoment = new DicomTag(0x0015, 0x000F, GLCMFeatureType.InverseDifferenceMoment.ToString());
        public static readonly DicomTag NormalizedInverseDifferenceMoment = new DicomTag(0x0015, 0x0010, GLCMFeatureType.NormalizedInverseDifferenceMoment.ToString());
        public static readonly DicomTag InverseVariance = new DicomTag(0x0015, 0x0011, GLCMFeatureType.InverseVariance.ToString());
        public static readonly DicomTag Correlation = new DicomTag(0x0015, 0x0012, GLCMFeatureType.Correlation.ToString());
        public static readonly DicomTag Autocorrection = new DicomTag(0x0015, 0x0013, GLCMFeatureType.Autocorrection.ToString());
        public static readonly DicomTag ClusterTendency = new DicomTag(0x0015, 0x0014, GLCMFeatureType.ClusterTendency.ToString());
        public static readonly DicomTag ClusterShade = new DicomTag(0x0015, 0x0015, GLCMFeatureType.ClusterShade.ToString());
        public static readonly DicomTag ClusterProminence = new DicomTag(0x0015, 0x0016, GLCMFeatureType.ClusterProminence.ToString());
        public static readonly DicomTag InformationalMeasureOfCorrelation1 = new DicomTag(0x0015, 0x0017, GLCMFeatureType.InformationalMeasureOfCorrelation1.ToString());
        public static readonly DicomTag InformationalMeasureOfCorrelation2 = new DicomTag(0x0015, 0x0018, GLCMFeatureType.InformationalMeasureOfCorrelation2.ToString());
        public static readonly DicomTag MCC = new DicomTag(0x0015, 0x0019, GLCMFeatureType.MCC.ToString());

        //一阶特征
        public static readonly DicomTag Mean = new DicomTag(0x0017, 0x0001, FirstOrderFeatureType.Mean.ToString());
        public static readonly DicomTag Variance = new DicomTag(0x0017, 0x0002, FirstOrderFeatureType.Variance.ToString());
        public static readonly DicomTag Skewness = new DicomTag(0x0017, 0x0003, FirstOrderFeatureType.Skewness.ToString());
        public static readonly DicomTag Kurtosis = new DicomTag(0x0017, 0x0004, FirstOrderFeatureType.Kurtosis.ToString());
        public static readonly DicomTag Median = new DicomTag(0x0017, 0x0005, FirstOrderFeatureType.Median.ToString());
        public static readonly DicomTag Minimum = new DicomTag(0x0017, 0x0006, FirstOrderFeatureType.Minimum.ToString());
        public static readonly DicomTag Percentile10 = new DicomTag(0x0017, 0x0007, FirstOrderFeatureType.Percentile10.ToString());
        public static readonly DicomTag Percentile90 = new DicomTag(0x0017, 0x0008, FirstOrderFeatureType.Percentile90.ToString());
        public static readonly DicomTag Maximum = new DicomTag(0x0017, 0x0009, FirstOrderFeatureType.Maximum.ToString());
        public static readonly DicomTag Interquartile = new DicomTag(0x0017, 0x000A, FirstOrderFeatureType.Interquartile.ToString());
        public static readonly DicomTag Range = new DicomTag(0x0017, 0x000B, FirstOrderFeatureType.Range.ToString());
        public static readonly DicomTag MeanAbsoluteDeviation = new DicomTag(0x0017, 0x000C, FirstOrderFeatureType.MeanAbsoluteDeviation.ToString());
        public static readonly DicomTag RobustMeanAbsoluteDeviation = new DicomTag(0x0017, 0x000D, FirstOrderFeatureType.RobustMeanAbsoluteDeviation.ToString());
        public static readonly DicomTag Peak = new DicomTag(0x0017, 0x000E, FirstOrderFeatureType.Peak.ToString());
        public static readonly DicomTag Energy = new DicomTag(0x0017, 0x0011, FirstOrderFeatureType.Energy.ToString());
        public static readonly DicomTag RootMeanSquared = new DicomTag(0x0017, 0x0012, FirstOrderFeatureType.RootMeanSquared.ToString());
        public static readonly DicomTag TotalEnergy = new DicomTag(0x0017, 0x0013, FirstOrderFeatureType.TotalEnergy.ToString());
        public static readonly DicomTag StandardDeviation = new DicomTag(0x0017, 0x0014, FirstOrderFeatureType.StandardDeviation.ToString());
        public static readonly DicomTag Uniformity = new DicomTag(0x0017, 0x0015, FirstOrderFeatureType.Uniformity.ToString());
        public static readonly DicomTag Entropy = new DicomTag(0x0017, 0x0016, FirstOrderFeatureType.Entropy.ToString());

        private static Dictionary<DicomTag, DicomVR> vrMaps = new Dictionary<DicomTag, DicomVR>();

        static PrivateDicomTag()
        {
            vrMaps.Add(Preprocess,DicomVR.LO);
            vrMaps.Add(Interpolation2D, DicomVR.SL);
            vrMaps.Add(Interpolation3D, DicomVR.SL);
            vrMaps.Add(MaskPartialVolumeThreshold, DicomVR.FD);
            vrMaps.Add(BinWidth, DicomVR.FD);
            vrMaps.Add(IVHBinWidth, DicomVR.FD);
            vrMaps.Add(NormalizeScale, DicomVR.FD);
            vrMaps.Add(RangeMax, DicomVR.FD);
            vrMaps.Add(RangeMin, DicomVR.FD);
            vrMaps.Add(Rlpha, DicomVR.FD);
            vrMaps.Add(DeltaGLCM, DicomVR.FD);
            vrMaps.Add(DeltaNGTDM, DicomVR.FD);
            vrMaps.Add(DeltaNGLDM, DicomVR.FD);
            vrMaps.Add(BoxSizes, DicomVR.IS);
            vrMaps.Add(ResamplingFactorXYZ, DicomVR.OD);
            vrMaps.Add(Force2D, DicomVR.SL);
            vrMaps.Add(WaveletFilterName, DicomVR.LO);
            vrMaps.Add(EnableGLCM, DicomVR.SL);
            vrMaps.Add(EnableFirstOrder, DicomVR.SL);
            vrMaps.Add(RangeFilterMode, DicomVR.SL);
            vrMaps.Add(UseFixedBins, DicomVR.SL);
            vrMaps.Add(NBins, DicomVR.SL);

            vrMaps.Add(MaximumProbability, DicomVR.FD);
            vrMaps.Add(JointAverage, DicomVR.FD);
            vrMaps.Add(SumSquares, DicomVR.FD);
            vrMaps.Add(JointEntropy, DicomVR.FD);
            vrMaps.Add(DifferenceAverage, DicomVR.FD);
            vrMaps.Add(DifferenceVariance, DicomVR.FD);
            vrMaps.Add(DifferenceEntropy, DicomVR.FD);
            vrMaps.Add(SumAverage, DicomVR.FD);
            vrMaps.Add(SumVariance, DicomVR.FD);
            vrMaps.Add(SumEntropy, DicomVR.FD);
            vrMaps.Add(JointEnergy, DicomVR.FD);
            vrMaps.Add(Contrast, DicomVR.FD);
            vrMaps.Add(InverseDifference, DicomVR.FD);
            vrMaps.Add(NormalizedInverseDifference, DicomVR.FD);
            vrMaps.Add(InverseDifferenceMoment, DicomVR.FD);
            vrMaps.Add(NormalizedInverseDifferenceMoment, DicomVR.FD);
            vrMaps.Add(InverseVariance, DicomVR.FD);
            vrMaps.Add(Correlation, DicomVR.FD);
            vrMaps.Add(Autocorrection, DicomVR.FD);
            vrMaps.Add(ClusterShade, DicomVR.FD);
            vrMaps.Add(ClusterTendency, DicomVR.FD);
            vrMaps.Add(ClusterProminence, DicomVR.FD);
            vrMaps.Add(InformationalMeasureOfCorrelation1, DicomVR.FD);
            vrMaps.Add(InformationalMeasureOfCorrelation2, DicomVR.FD);
            vrMaps.Add(MCC, DicomVR.FD);
            vrMaps.Add(Mean,DicomVR.FD);vrMaps.Add(Variance, DicomVR.FD);vrMaps.Add(Skewness, DicomVR.FD);
            vrMaps.Add(Kurtosis, DicomVR.FD); vrMaps.Add(Median, DicomVR.FD); vrMaps.Add(Minimum, DicomVR.FD);
            vrMaps.Add(Percentile10, DicomVR.FD); vrMaps.Add(Percentile90, DicomVR.FD); vrMaps.Add(Maximum, DicomVR.FD);
            vrMaps.Add(Interquartile, DicomVR.FD); vrMaps.Add(Range, DicomVR.FD); vrMaps.Add(MeanAbsoluteDeviation, DicomVR.FD);
            vrMaps.Add(RobustMeanAbsoluteDeviation, DicomVR.FD); vrMaps.Add(Peak, DicomVR.FD); vrMaps.Add(Energy, DicomVR.FD);
            vrMaps.Add(RootMeanSquared, DicomVR.FD); vrMaps.Add(TotalEnergy, DicomVR.FD); vrMaps.Add(StandardDeviation, DicomVR.FD); vrMaps.Add(Uniformity, DicomVR.FD);
            vrMaps.Add(Entropy, DicomVR.FD);
        }
        

        public static void AddOrUpdate(DicomDataset dataset, DicomTag tag, object value)
        {
            DicomVR dicomVR = vrMaps[tag];
            switch (dicomVR.Code)
            {
                case "OD":
                    dataset.AddOrUpdate(dicomVR, tag, (double[])value);
                    break;
                case "IS":
                    dataset.AddOrUpdate(dicomVR, tag, (int[])value);
                    break;
                case "SL":
                    dataset.AddOrUpdate(dicomVR, tag, (int)value);
                    break;
                case "FD":
                    dataset.AddOrUpdate(dicomVR, tag, (double)value);
                    break;
                default:
                    dataset.AddOrUpdate(dicomVR, tag, (string)value);
                    break;

            }
        }
        
        //根据分组获取私有标签
        public static List<DicomTag> GetPrivateDicomTagByGroupId(ushort groupId)
        {
            return vrMaps.Keys.Where(c => c.Group == groupId).ToList();
        }
        public static DicomTag GetPrivateDicomTagByName(string name) 
        {
            return vrMaps.Keys.Where(c=>c.PrivateCreator.Creator.Equals(name)).First();
        }
    }
}
