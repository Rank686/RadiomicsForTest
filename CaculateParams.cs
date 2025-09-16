using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net
{
    public class CaculateParams
    {
        public string[] Preprocess = new string[0];
        public int FilterMode = 0;  //值范围过滤方式：0绝对值，1相对值，2标准差
        public double Label = 1.0;
        public double NormalizeScale = 1.0;
        public double[] ResamplingFactorXYZ;
        public int PadDistance = 5;//重采样后扩充ROI大小
        public bool Force2D = true;
        public int Interpolation2D = 1;//插值方法：NEAREST_NEIGHBOR=0, BILINEAR=1, BICUBIC=2;
        public double MaskPartialVolumeThreshold = 0.5;
        public bool UseFixedBinNumber = false;//直方图固定区间数
        public double BinWidth = 25;
        public int NBins = 44;//区间个数
        public bool Normalize = false;
        public double RangeMax = Double.NaN;
        public double RangeMin = Double.NaN;
        public double IVHBinWidth = 25;
        public int GLCMDelta = 1;//GLCM计算距离
        public string GLCMWeightingMethod;
        public string WaveFilterName = "";//图像滤波器名称，默认haar
        public double DensityShift = 1000d;//计算一阶特征值Energy时，所有灰度值加上某个固定数，从而避免出现负数
        public bool EnableGLCM = false;
        public bool EnableFirstOrder = false;


        //解析计算参数
        public static CaculateParams ParseParams(DicomDataset ds)
        {
            CaculateParams caculateParams = new CaculateParams();
            List<DicomTag> tags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0013);
            bool result = false;
            string sValue;
            int iValue;
            double dValue;
            int[] iArray;
            double[] dArray;
            string[] sArray;

            foreach (DicomTag tag in tags)
            {
                if (tag.PrivateCreator.Creator.Equals("Interpolation2D"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.Interpolation2D = iValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("MaskPartialVolumeThreshold"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.MaskPartialVolumeThreshold = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("UseFixedBins"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.UseFixedBinNumber = Convert.ToBoolean(iValue);
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("NBins"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.NBins = iValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("BinWidth"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.BinWidth = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("IVHBinWidth"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.IVHBinWidth = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("NormalizeScale"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.NormalizeScale = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("FilterMode"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.NormalizeScale = iValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("RangeMax"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.RangeMax = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("RangeMin"))
                {
                    result = ds.TryGetValue<double>(tag, 0, out dValue);
                    if (result)
                    {
                        caculateParams.RangeMin = dValue;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("ResamplingFactorXYZ"))
                {
                    result = ds.TryGetValues<double>(tag, out dArray);
                    if (result)
                    {
                        caculateParams.ResamplingFactorXYZ = dArray;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("Force2D"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.Force2D = Convert.ToBoolean(iValue);
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("Preprocess"))
                {
                    result = ds.TryGetValues<string>(tag, out sArray);
                    if (result)
                    {
                        caculateParams.Preprocess = sArray;
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("EnableGLCM"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.EnableGLCM = Convert.ToBoolean(iValue);
                    }
                }
                else if (tag.PrivateCreator.Creator.Equals("EnableFirstOrder"))
                {
                    result = ds.TryGetValue<int>(tag, 0, out iValue);
                    if (result)
                    {
                        caculateParams.EnableFirstOrder = Convert.ToBoolean(iValue);
                    }
                }
            }
            return caculateParams;
        }
    }
}
