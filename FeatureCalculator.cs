using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Writer;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Radiomics.Net.Exceptions;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net
{
    public class FeatureCalculator
    {
        public static ResultMessage Calclate(List<DicomDataset> dicoms)
        {
            if (dicoms.Count == 0 || dicoms.Count % 2 != 0)
            {
                throw new CustomException((int)Errors.ParamsError, "调用参数个数错误");
            }
            ImagePlus currentImp = new ImagePlus(dicoms.Take(dicoms.Count / 2).ToList());
            ImagePlus currentMask = new ImagePlus(dicoms.Skip(dicoms.Count / 2).ToList());
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            string filePath = string.Format(@"logs\files\{0}", DateTime.Now.ToString("yyMMddHHmmss"));
            try
            {
                CaculateParams caculateParams = CaculateParams.ParseParams(dicoms[0]);

                //校验ROI是否存在
                if (Utils.isBlankMaskStack(currentMask, (int)caculateParams.Label))
                {
                    throw new CustomException((int)Errors.ROIDataError, "ROI数据无有效区域");
                }
                ImagePlus[] imgs=new ImagePlus[] { currentImp, currentMask};
                //数据标准化
                if (caculateParams.Preprocess.Contains("Normalize"))
                {
                    imgs[0] = ImagePreprocessing.Normalize(imgs[0], imgs[1], caculateParams);
                }
                //重采样
                if (caculateParams.Preprocess.Contains("Resample") && caculateParams.ResamplingFactorXYZ != null)
                {
                    imgs = ImagePreprocessing.Resample(imgs[0], imgs[1], caculateParams.ResamplingFactorXYZ, caculateParams);
                }
                ImagePlus resampleImp = imgs[0];
                ImagePlus resampleMask = imgs[1];
                double[] voxels = Utils.GetVoxels(resampleImp, resampleMask, (int)caculateParams.Label);
                //范围过滤
                if (caculateParams.Preprocess.Contains("RangeFilter") && !double.IsNaN(caculateParams.RangeMax) && !double.IsNaN(caculateParams.RangeMin))
                {
                    int filterMode = caculateParams.FilterMode;
                    if (filterMode == 0)
                    {
                        resampleMask = ImagePreprocessing.RangeFiltering(resampleImp, resampleMask, (int)caculateParams.Label, caculateParams.RangeMax, caculateParams.RangeMin);
                    }
                    else if (filterMode == 1)
                    {
                        double maxValue = Statistics.Maximum(voxels);
                        resampleMask = ImagePreprocessing.RangeFiltering(resampleImp, resampleMask, (int)caculateParams.Label, caculateParams.RangeMax * maxValue, caculateParams.RangeMin * maxValue);
                    }
                    else if (filterMode == 2)
                    {
                        resampleMask = ImagePreprocessing.OutlierFiltering(resampleImp, resampleMask, (int)caculateParams.Label, caculateParams);
                    }
                    if (resampleMask == null)
                    {
                        resampleMask = currentMask;
                    }
                }
                //图像滤波处理
                if(caculateParams.WaveFilterName.Length > 0) 
                {
                    resampleImp = ImagePreprocessing.FwtTransform(resampleImp, caculateParams);
                }
                //裁剪ROI区域
                imgs = ImagePreprocessing.Crop(resampleImp, resampleMask, caculateParams);
                resampleImp = imgs[0];
                resampleMask = imgs[1];
                //灰度值离散化
                ImagePlus discImg = null;
                if (caculateParams.UseFixedBinNumber)
                {
                    discImg = Utils.Discrete(resampleImp, resampleMask, (int)caculateParams.Label, caculateParams.NBins);
                }
                else
                {
                    discImg = Utils.DiscreteByBinWidth(resampleImp, resampleMask, (int)caculateParams.Label, caculateParams);
                    caculateParams.NBins = Utils.GetNumOfBinsByMax(discImg, resampleMask, (int)caculateParams.Label);
                }
                //计算GLCM
                if (caculateParams.EnableGLCM)
                {
                    GLCMFeatures glcmFeatures = new GLCMFeatures(resampleImp, resampleMask, discImg, caculateParams);
                    foreach (var name in Enum.GetNames(typeof(GLCMFeatureType)))
                    {
                        double result = glcmFeatures.Calculate((GLCMFeatureType)Enum.Parse(typeof(GLCMFeatureType), name));
                        PrivateDicomTag.AddOrUpdate(dicoms[0], PrivateDicomTag.GetPrivateDicomTagByName(name), result);
                    }
                }
                //计算一阶特征
                if (caculateParams.EnableFirstOrder)
                {
                    FirstOrderFeatures features = new FirstOrderFeatures(resampleImp, resampleMask, discImg, caculateParams);
                    foreach (var name in Enum.GetNames(typeof(FirstOrderFeatureType)))
                    {
                        double result = features.Calculate((FirstOrderFeatureType)Enum.Parse(typeof(FirstOrderFeatureType), name));
                        PrivateDicomTag.AddOrUpdate(dicoms[0], PrivateDicomTag.GetPrivateDicomTagByName(name), result);
                    }
                }
            }
            catch (CustomException myException) 
            {
                logger.Error(myException);
                //new DicomFile(dicoms[0]).Save(filePath + "_i.dcm");
                //new DicomFile(dicoms[1]).Save(filePath + "_m.dcm");
                return myException.Result;
            }catch (Exception e) 
            {
                logger.Error(e);
                //new DicomFile(dicoms[0]).Save(filePath + "_i.dcm");
                //new DicomFile(dicoms[1]).Save(filePath + "_m.dcm");
                return new ResultMessage((int)Errors.SystemError, e.Message);
            }
            return new ResultMessage(Errors.OK);
        }
    }
}
