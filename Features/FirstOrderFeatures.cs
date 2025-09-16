using MathNet.Numerics.Statistics;
using Radiomics.Net.ImageProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Features
{
    public class FirstOrderFeatures
    {
        /**
	     * for Energy, TotalEnergy, RMS.
	     * same as voxel array shift.
	     * If using CT data, or data normalized with mean 0, consider setting this parameter to a fixed value (e.g. 2000) that ensures non-negative numbers in the image.
	     */
        private double densityShift = 0d;//in unit value !
        /**
         * mask label value
         */
        private int label;
        private int nBins;
        ImagePlus orgImg;
        ImagePlus orgMask;
        ImagePlus discImg;
        //ROI内所有像素灰度值
        private double[] voxels = null;
        //ROI内直方图数据
        int[] hist = null;
        public FirstOrderFeatures(ImagePlus img, ImagePlus mask, ImagePlus discImg, CaculateParams caculateParams)
        {
            this.orgImg = img;
            this.orgMask = mask;
            this.label = (int)caculateParams.Label;
            this.densityShift = caculateParams.DensityShift;
            this.nBins = caculateParams.NBins;
            if (discImg != null)
            {
                this.discImg = discImg;
            }
            else
            {
                if (caculateParams.UseFixedBinNumber)
                {
                    this.discImg = Utils.Discrete(orgImg, orgMask, this.label, this.nBins);
                }
                else
                {
                    this.discImg = Utils.DiscreteByBinWidth(orgImg, orgMask, this.label, caculateParams);
                    this.nBins = Utils.GetNumOfBinsByMax(this.discImg, orgMask, this.label);
                }
            }
            voxels = Utils.GetVoxels(this.orgImg, orgMask, this.label);
            hist = Utils.GetHistogram(Utils.GetVoxels(this.discImg, orgMask, this.label));
        }
        public double Calculate(FirstOrderFeatureType featureType)
        {
            switch (featureType)
            {
                case FirstOrderFeatureType.Energy: return GetEnergy();
                case FirstOrderFeatureType.TotalEnergy: return GetTotalEnergy();
                case FirstOrderFeatureType.Maximum: return GetMaximum();
                case FirstOrderFeatureType.Minimum: return GetMinimum();
                case FirstOrderFeatureType.Peak: return GetPeak();
                case FirstOrderFeatureType.Mean: return GetMean();
                case FirstOrderFeatureType.Range: return GetMinMaxRange();
                case FirstOrderFeatureType.Median:  return GetMedian();
                case FirstOrderFeatureType.Variance: return GetVariance();
                case FirstOrderFeatureType.Skewness:return GetSkewness();
                case FirstOrderFeatureType.Kurtosis: return GetKurtosis();
                case FirstOrderFeatureType.Uniformity: return GetUniformity();
                case FirstOrderFeatureType.Entropy: return GetEntropy();
                case FirstOrderFeatureType.Percentile10:return GetPercentile(10);
                case FirstOrderFeatureType.Percentile90:return GetPercentile(90);
                case FirstOrderFeatureType.Interquartile:return GetPercentile(75)-GetPercentile(25);
                case FirstOrderFeatureType.MeanAbsoluteDeviation: return GetMeanAbsoluteDeviation();
                case FirstOrderFeatureType.RobustMeanAbsoluteDeviation: return GetRobustMeanAbsoluteDeviation();
                case FirstOrderFeatureType.RootMeanSquared: return GetRootMeanSquared();
                case FirstOrderFeatureType.StandardDeviation: return GetStandardDeviation();
                default:
                    return double.NaN;
            }
        }
        public double GetEnergy()
        {
            double energy = 0d;
            int size = voxels.Length;
            for (int i = 0; i < size; i++)
            {
                double v = voxels[i] + densityShift;
                if (v != 0.0)
                {
                    v = Math.Pow(v, 2);
                    energy += v;
                }
            }
            return energy;
        }

        public double GetTotalEnergy()
        {
            double energy = GetEnergy();
            double voxelSize = orgImg.PixelWidth * orgImg.PixelHeight * orgImg.PixelDepth;
            if (voxelSize > 0.0)
            {
                return voxelSize * energy;
            }
            else
            {
                return energy;
            }
        }
        public double GetMinimum()
        {
            return Statistics.Minimum(voxels);
        }

        public double GetMaximum()
        {
            return Statistics.Maximum(voxels);
        }

        public double GetMean()
        {
            double sum = 0d;
            int numOfVoxel = voxels.Length;
            for (int i = 0; i < numOfVoxel; i++)
            {
                sum += voxels[i];
            }
            return numOfVoxel != 0d ? sum / numOfVoxel : double.NaN;
        }

        public double GetMedian()
        {
            double median = double.NaN;
            if (voxels.Length % 2 == 0)
                median = ((double)voxels[voxels.Length / 2] + (double)voxels[voxels.Length / 2 - 1]) / 2;
            else
                median = (double)voxels[voxels.Length / 2];
            return median;
        }

        public double GetMinMaxRange()
        {
            double min = GetMinimum();
            double max = GetMaximum();
            return max - min;
        }
        public double GetVariance()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            double n = voxels.Length;
            double sumsq = 0d;
            double mean = Statistics.Mean(voxels);
            foreach (double v in voxels)
            {
                sumsq += Math.Pow(v - mean, 2);
            }
            return sumsq / n;
        }

        public double GetSkewness()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            int pixelCount = voxels.Length;
            double mean = Statistics.Mean(voxels);
            double sum2 = 0d;//sum of pow(v-mean,2) 
            double sum3 = 0d;//sum of pow(v-mean,3)
            for (int i = 0; i < pixelCount; i++)
            {
                sum2 += Math.Pow((voxels[i] - mean), 2);
                sum3 += Math.Pow((voxels[i] - mean), 3);
            }
            //		System.out.println("skewness1 : "+(sum3/pixelCount)/Math.pow(Math.sqrt(sum2/pixelCount), 3));//pyradiomics, same result of IJ
            //		System.out.println("skewness1 : "+(sum3/pixelCount)/Math.pow(sum2/(pixelCount+0.0), 3.0/2.0));//IBSI, really almost same as IJ.
            return (sum3 / (double)pixelCount) / Math.Pow(Math.Sqrt(sum2 / (double)pixelCount), 3);
        }
        public double GetKurtosis()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            int pixelCount = voxels.Length;
            double sum2 = 0.0, sum4 = 0.0;
            double mean = Statistics.Mean(voxels);
            for (int i = 0; i < pixelCount; i++)
            {
                sum2 += Math.Pow((voxels[i] - mean), 2);
                sum4 += Math.Pow((voxels[i] - mean), 4);
            }
            return (sum4 / pixelCount) / Math.Pow(sum2 / pixelCount, 2);
        }
        public double GetMeanAbsoluteDeviation()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            double absSum = 0d;
            double mean = GetMean();
            for (int i = 0; i < voxels.Length; i++)
            {
                absSum += Math.Abs(voxels[i] - mean);
            }
            // Return mean absolute deviation about mean.
            return absSum / voxels.Length;
        }
        public double GetRobustMeanAbsoluteDeviation()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            double absSum = 0d;
            List<Double> percenileArr = new List<Double>();
            double p10 = Statistics.Percentile(voxels, 10);
            double p90 = Statistics.Percentile(voxels, 90);
            for (int i = 0; i < voxels.Length; i++)
            {
                if (voxels[i] >= p10 && voxels[i] <= p90)
                {
                    percenileArr.Add(voxels[i]);
                }
            }
            double[] perArr = percenileArr.ToArray();
            double mean = Statistics.Mean(perArr);
            for (int i = 0; i < perArr.Length; i++)
            {
                absSum += Math.Abs(perArr[i] - mean);
            }
            // Return mean absolute deviation about mean.
            return absSum / perArr.Length;
        }
        public double GetRootMeanSquared()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            /*
             * this is because to considering densityShift,
             * use getEnergy.
             */
            return Math.Sqrt(GetEnergy() / voxels.Length);
        }
        public double GetStandardDeviation()
        {
            if (voxels == null || voxels.Length == 0)
            {
                return double.NaN;
            }
            double n = voxels.Length;
            double sumsq = 0d;
            double mean = Statistics.Mean(voxels);
            foreach (double v in voxels)
            {
                sumsq += Math.Pow(v - mean, 2);
            }
            double var = sumsq / n;
            return Math.Sqrt(var);
        }
        public double GetPercentile(int p_th)
        {
            if (voxels == null || voxels.Length < 1)
            {
                return double.NaN;
            }
            //		return StatUtils.percentile(voxelArray, p_th);//Compute the estimated percentile
            //		return new Percentile().evaluate(voxelArray, p_th);//Compute the estimated percentile
            //		int index = (int) Math.ceil((double)(p_th / 100.0) * (double)voxelArray.length);
            int index = (int)Math.Floor((double)(p_th / 100.0) * (double)voxels.Length);
            if (index >= voxels.Length)
            {
                index = voxels.Length - 1;
            }
            else if (index < 0)
            {
                index = 0;
            }
            return voxels[index];
        }
        public double GetPeak() 
        {
            double peak = double.MinValue;
            for (int z = 0; z < orgImg.Slice; z++) 
            {
                for (int y = 0; y < orgImg.Height; y++) 
                {
                    for (int x = 0; x < orgImg.Width; x++) 
                    {
                        if ((int)orgMask.GetXYZ(x, y, z) == this.label) 
                        {
                            List<double> voxels = GetSphericalVoxels(x, y, z, 1);
                            if (voxels.Count == 0)
                            {
                                double val = orgImg.GetXYZ(x, y, z);
                                if (val > peak) 
                                {
                                    peak = val;
                                }
                            }
                            else
                            {
                                double res = Statistics.Mean(voxels);
                                if (res > peak)
                                {
                                    peak = res;
                                }
                            }
                        }
                    }
                }
            }
            if(peak==double.MinValue) 
               return double.NaN;
            else
               return peak;
        }
        //获取周边区域像素值
        private List<double> GetSphericalVoxels(int x, int y, int z,int distance) 
        {
            List<double> voxels = new List<double>();
            for (int pz = z - distance; pz <= z + distance; pz++) 
            {
                if (pz < 0 || pz >= orgImg.Slice) continue;
                for (int py = y - distance; py <= y + distance; py++) 
                {
                    if (py < 0 || py >= orgImg.Height) continue;
                    for (int px = x - distance; px <= x + distance; px++) 
                    {
                        if (px < 0 || px >= orgImg.Width) continue;
                        double length = Math.Sqrt(Math.Pow((px - x), 2) + Math.Pow((py - y), 2) + Math.Pow((pz - z), 2));
                        //取相邻四格（十字）区域内均值，不包含该像素本身，排除孤点极值
                        if (length > distance || length==0)
                        {
                            continue;
                        }
                        voxels.Add(orgImg.GetXYZ(px, py, pz));
                    }
                }
            }
            return voxels;
        }
        private double GetEntropy()
        {
            if (hist == null)
            {
                return double.NaN;
            }
            double totalpixel = voxels.Length;
            Double ent = 0d;
            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i] > 0)
                {
                    double p = (double)hist[i] / (double)totalpixel;
                    if (p == 0.0)
                    {
                        continue;
                    }
                    ent -= p * ((Math.Log(p) / Math.Log(2.0)));
                }
            }
            return ent;
        }
        /**
	     * Note that this feature is sometimes referred to as energy.
	     * @return
	    */
        public double GetUniformity()
        {
            double totalpixel = voxels.Length;
            Double uni = 0d;
            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i] > 0)
                {
                    double p = (double)hist[i] / (double)totalpixel;
                    uni += Math.Pow(p, 2);
                }
            }
            return uni;
        }

    }
}
