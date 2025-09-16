using MathNet.Numerics.Statistics;
using Radiomics.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.ImageProcess
{
    public class ImagePreprocessing
    {
        public static ImagePlus Normalize(ImagePlus imp, ImagePlus mask, CaculateParams caculateParams)
        {
            int label = (int)caculateParams.Label;
            double scale = caculateParams.NormalizeScale;
            int w = imp.Width;
            int h = imp.Height;
            int s = imp.Slice;
            //double[] voxels = Utils.GetVoxels(imp, mask, label);
            double[] voxels = imp.Stack[0];
            double mean = voxels.Mean();
            double stdDev = Math.Sqrt(voxels.Variance());
            ImagePlus stdImp = imp.Copy();
            for (int z = 0; z < s; z++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int lbl = (int)mask.GetXYZ(x, y, z);
                        //if (lbl != label)
                        //{
                        //    stdImp.SetXYZ(x, y, z, double.NaN);
                        //    continue;
                        //}
                        double v = imp.GetXYZ(x, y, z);
                        double sv = (v - mean) / stdDev;
                        stdImp.SetXYZ(x, y, z, sv * scale);//apply scaling
                    }
                }
            }
            return stdImp;
        }
        public static ImagePlus[] Resample(ImagePlus img, ImagePlus mask, double[] resamplingFactorXYZ, CaculateParams caculateParams)
        {
            ImagePlus resampledImp = null;
            ImagePlus resampledMask = null;

            if (caculateParams.Force2D)
            {
                //ignore z
                resampledImp = Utils.Resample2D(img, false, resamplingFactorXYZ[0], resamplingFactorXYZ[1], caculateParams);
                //Mask采用邻近插值法
                int interpolation2D = caculateParams.Interpolation2D;
                caculateParams.Interpolation2D = 0;
                resampledMask = Utils.Resample2D(mask, true, resamplingFactorXYZ[0], resamplingFactorXYZ[1], caculateParams);
                caculateParams.Interpolation2D = interpolation2D;
            }
            //else
            //{
            //    // trilinear interpolation
            //    resampledImp = Utils.resample3D(img, false, resamplingFactorXYZ[0], resamplingFactorXYZ[1], resamplingFactorXYZ[2]);
            //    resampledMask = Utils.resample3D(mask, true, resamplingFactorXYZ[0], resamplingFactorXYZ[1], resamplingFactorXYZ[2]);
            //}
            return new ImagePlus[] { resampledImp, resampledMask };
        }
        public static ImagePlus[] Crop(ImagePlus img, ImagePlus mask, CaculateParams caculateParams) 
        {
            Dictionary<string, double[]> bounds = Utils.GetRoiBoundingBoxInfo(mask, (int)caculateParams.Label);
            int width = (int)(bounds["x"][1] - bounds["x"][0] + 1);
            int height = (int)(bounds["y"][1] - bounds["y"][0] + 1);
            int slice = (int)(bounds["z"][1] - bounds["z"][0] + 1);
            int roiX = (int)bounds["x"][0];
            int roiY = (int)bounds["y"][0];
            int roiz = (int)bounds["z"][0];
            ImagePlus cropImp = new ImagePlus(width, height, slice);
            ImagePlus cropMask=new ImagePlus(width, height, slice);
            cropImp.PixelHeight = img.PixelHeight;
            cropImp.PixelWidth = img.PixelWidth;
            cropImp.PixelDepth= img.PixelDepth;
            cropImp.BitsAllocated = img.BitsAllocated;
            cropMask.PixelHeight = mask.PixelHeight;
            cropMask.PixelWidth = mask.PixelWidth;
            cropMask.PixelDepth = mask.PixelDepth;
            cropMask.BitsAllocated = mask.BitsAllocated;
            for (int z = roiz; z < roiz + slice; z++) 
            {
                for (int y = roiY; y < roiY + height; y++) 
                {
                    for (int x = roiX; x < roiX + width; x++) 
                    {
                        cropImp.Stack[z-roiz][(y-roiY) * width + x - roiX] = img.Stack[z][y * img.Width + x];
                        cropMask.Stack[z-roiz][(y - roiY) * width + x - roiX] = mask.Stack[z][y * mask.Width + x];
                    }
                }
            }
            return new ImagePlus[] { cropImp, cropMask };
        }

        /**
	 * keep original pixels values. but replace upper and lower values using z-score. 
	 * @param imp
	 * @param mask
	 * @param label
	 * @return
	 */
        public static ImagePlus OutlierFiltering(ImagePlus imp, ImagePlus mask, int label, CaculateParams caculateParams)
        {
            int w = imp.Width;
            int h = imp.Height;
            int s = imp.Slice;
            double[] voxels = Utils.GetVoxels(imp, mask, label);
            double mean = voxels.Mean();
            double n = voxels.Length;
            double sumsq = 0d;
            foreach (double v in voxels)
            {
                sumsq += Math.Pow(v - mean, 2);
            }
            double stdDev = Math.Sqrt(sumsq / n);
            //		double stdDev = Math.sqrt(StatUtils.variance(voxels));//almost same 

            double outlierUpper = mean + caculateParams.RangeMin * stdDev;//calibrated
            double outlierLower = mean - caculateParams.RangeMax * stdDev;//calibrated
                                                          //		double maxRaw = imp.getCalibration().getRawValue(outlierUpper);//back scale to raw
                                                          //		double minRaw = imp.getCalibration().getRawValue(outlierLower);
            ImagePlus newMask = mask.Copy();
            for (int z = 0; z < s; z++)
            {
                //dup is a new image, so without calibaration is OK at here.   
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int lbl = (int)mask.GetXYZ(x, y, z);//IJ coordinate
                        double v = imp.GetXYZ(x, y, z);
                        if (lbl != label || v > outlierUpper || v < outlierLower)
                        {
                            newMask.SetXYZ(x, y, z, 0);
                        }
                        else if (lbl == label)
                        {
                            newMask.SetXYZ(x, y, z, 1);
                        }
                        else
                        {
                            newMask.SetXYZ(x, y, z, 0);
                        }
                    }
                }
            }
            return newMask;
        }

        public static ImagePlus RangeFiltering(ImagePlus imp, ImagePlus mask, int label, double rangeMax, double rangeMin)
        {
            if (rangeMax <= rangeMin)
            {
                throw new CustomException(Errors.RangeFilterParamsError);
            }
            int w = imp.Width;
            int h = imp.Height;
            int s = imp.Slice;
            ImagePlus newMask = mask.Copy();
            int included = 0;
            int excluded = 0;
            for (int z = 0; z < s; z++)
            {

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int lbl = (int)mask.GetXYZ(x, y, z);
                        double v = imp.GetXYZ(x, y, z);
                        if (lbl != label)
                        {
                            newMask.SetXYZ(x, y, z, 0);
                        }
                        else
                        {
                            if (!(v < rangeMin) && !(v > rangeMax))
                            {
                                newMask.SetXYZ(x, y, z, lbl);
                                included++;
                            }
                            else
                            {
                                newMask.SetXYZ(x, y, z, 0);
                                excluded++;
                            }
                        }
                    }
                }
            }
            return newMask;
        }
        public static ImagePlus PreprocessDiscretise(ImagePlus resampled, ImagePlus resegmentedMask, int targetLabel, CaculateParams caculateParams)
        {
            ImagePlus discretiseImp = null;
            if (caculateParams.UseFixedBinNumber)
            {
                discretiseImp = Utils.Discrete(resampled, resegmentedMask, targetLabel, caculateParams.NBins);
            }
            else
            {
                /*
			     * Fixed Bin Width
			     */
                discretiseImp = Utils.DiscreteByBinWidth(resampled, resegmentedMask, targetLabel, caculateParams);
                caculateParams.NBins = Utils.GetNumOfBinsByMax(discretiseImp, resegmentedMask, targetLabel);
            }
            return discretiseImp;
        }
        public static ImagePlus CreateMask(ImagePlus image)
        {
            ImagePlus mask = image.Copy();
            for (int z = 0; z < image.Slice; z++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        mask.SetXYZ(x, y, z, 1);
                    }
                }
            }
            return mask;
        }
        //图像滤波，小波变换
        public static ImagePlus FwtTransform(ImagePlus imp, CaculateParams caculateParams)
        {
            ImageData imageData = new ImageData(imp);
            imageData.SetWaveletFilter(caculateParams.WaveFilterName);
            for (int z = 0; z < imp.Slice; z++) 
            {
                imageData.FwdTransform(z);
                //denoise here
                imageData.InvTransform(z);
            }
            ImagePlus newImage = imageData.GetFilterImage();
            return newImage;
        }
    }
}
