using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Radiomics.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.ImageProcess
{
    public class Utils
    {
        public static double[] GetVoxels(ImagePlus img, ImagePlus mask, int label)
        {
            List<double> voxels = new List<double>();
            int w = img.Width;
            int h = img.Height;
            int s = img.Slice;

            if (mask != null)
            {
                for (int z = 0; z < s; z++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int lbl_val = (int)mask.GetXYZ(x, y, z);
                            if (lbl_val == label)
                            {
                                voxels.Add(img.GetXYZ(x, y, z));
                            }
                        }
                    }
                }
            }
            else
            {//collect all voxels
                for (int z = 0; z < s; z++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            voxels.Add(img.GetXYZ(x, y, z));
                        }
                    }
                }
            }
            double[] voxelArray = voxels.ToArray();
            Array.Sort(voxelArray);
            return voxelArray;
        }
        public static ImagePlus Resample2D(ImagePlus imp, bool isMask, double resampleX, double resampleY, CaculateParams caculateParams)
        {
            if (imp == null)
            {
                return null;
            }
            double vx = imp.PixelWidth;
            double vy = imp.PixelHeight;
            if (vx == resampleX && vy == resampleY)
            {
                return imp;
            }
            if (resampleX == 0 || resampleY == 0)
            {
                throw new CustomException(Errors.ResampleParamsError);
            }

            double sx = vx / resampleX;
            double sy = vy / resampleY;
            int newW = (int)Math.Ceiling(imp.Width * sx);
            int newH = (int)Math.Ceiling(imp.Height * sy);
            int size = imp.Slice;
            ImagePlus resizeImg = new ImagePlus(newW, newH, size);
            for (int z = 0; z < size; z++)
            {

                double[] pixelData = imp.Resize2D(newW, newH, z, caculateParams);
                if (!isMask)
                {
                    resizeImg.Stack[z] = pixelData;
                }
                else
                {
                    for (int ny = 0; ny < newH; ny++)
                    {
                        for (int nx = 0; nx < newW; nx++)
                        {
                            if (pixelData[ny * newW + nx] > caculateParams.MaskPartialVolumeThreshold)
                            {
                                //if (nx >= lboundXNew && nx <= uboundXNew && ny >= lboundYNew && ny <= uboundYNew)
                                //{
                                pixelData[ny * newW + nx] = caculateParams.Label;//always 1
                                //}
                            }
                            else 
                            {
                                pixelData[ny * newW + nx] = 0;
                            }
                        }
                    }
                    resizeImg.Stack[z] = pixelData;
                }
            }
            resizeImg.PixelHeight = resampleY;
            resizeImg.PixelWidth = resampleX;
            resizeImg.PixelDepth = imp.PixelDepth;
            resizeImg.BitsAllocated = imp.BitsAllocated;
            return resizeImg;
        }
        public static ImagePlus Discrete(ImagePlus org, ImagePlus mask, int label, int nBins)
        {
            int w = org.Width;
            int h = org.Height;
            int s = org.Slice;
            ImagePlus discreImp = org.Copy();
            double[] voxels = GetVoxels(org, mask, label);//get voxels in Roi
            int numOfVoxel = voxels.Length;//voxels in Roi
            double max = voxels.Maximum();//max voxels in Roi
            double min = voxels.Minimum();//min voxels in Roi
            if (nBins < 1)
            {
                nBins = 1;
            }
            List<double> binEdges = new List<double>();
            double binWidth = (max - min) / nBins;
            double value = min;
            while (value <= max)
            {
                binEdges.Add(value);
                value = value + binWidth;
            }
            binEdges.Add(max + 1);
            int pixelCount = 0;
            for (int z = 0; z < s; z++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int lbl = (int)mask.GetXYZ(x, y, z);
                        double val = org.GetXYZ(x, y, z);
                        if (lbl == label)
                        {
                            int discVal = int.MinValue;
                            if (val >= max)
                            {
                                discVal = nBins;
                            }
                            else
                            {
                                discVal = FindBinIndex(val, binEdges);
                            }
                            discreImp.SetXYZ(x, y, z, discVal);
                            pixelCount++;
                        }
                        else
                        {
                            discreImp.SetXYZ(x, y, z, double.NaN);
                        }
                    }
                }
            }
            /*
             * validate whether all pixels catch-up.
             */
            if (numOfVoxel != pixelCount)
            {
                throw new CustomException(Errors.HistogramDataError);
            }
            return discreImp;
        }
        public static ImagePlus DiscreteByBinWidth(ImagePlus org, ImagePlus mask, int label, CaculateParams caculateParams)
        {
            int w = org.Width;
            int h = org.Height;
            int s = org.Slice;

            double[] voxels = GetVoxels(org, mask, label);//get voxels in Roi
            double max = voxels.Maximum();//max voxels in Roi
            double min = voxels.Minimum();//min voxels in Roi
            double modValue = min % caculateParams.BinWidth;
            if (modValue < 0) 
            {
                modValue += caculateParams.BinWidth;
            }
            double lowBound = min - modValue;
            double highBound = max + 2 * caculateParams.BinWidth;
            List<double> binEdges= new List<double>();
            double value = lowBound;
            while (value <= highBound) 
            {
                binEdges.Add(value);
                value = value + caculateParams.BinWidth;
            }
            if (Math.Abs(max - min) < caculateParams.BinWidth)
            {
                throw new CustomException((int)Errors.HistogramDataError, string.Format("数据范围{0}-{1}，而离散区间大小为{2}。",min,max,caculateParams.BinWidth));
            }

            ImagePlus discreImp = org.Copy();

            for (int z = 0; z < s; z++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int lbl = (int)mask.GetXYZ(x, y, z);
                        if (lbl == label)
                        {
                            double val = org.GetXYZ(x, y, z);
                            //int discVal = (int)((val - min) / caculateParams.BinWidth + 1f);//round by casting int.
                            int discVal = FindBinIndex(val, binEdges);
                            discreImp.SetXYZ(x, y, z, discVal);//then back to float.
                        }
                        else
                        {
                            discreImp.SetXYZ(x, y, z, double.NaN);
                        }
                    }
                }
            }
            return discreImp;
        }
        public static int FindBinIndex(double value, List<double> edgs) 
        {
            int index = 0;
            for (int i = 0; i < edgs.Count; i++)
            {
                index++;
                if (value >= edgs[i] && value < edgs[i + 1])
                {
                    break;
                }
            }
            return index;
        }
        public static int GetNumOfBinsByMax(ImagePlus discretisedImg, ImagePlus mask, int label)
        {
            double[] voxels = GetVoxels(discretisedImg, mask, label);//get voxels in Roi
            return (int)voxels.Maximum();//max voxels in Roi
        }
        public static bool isBlankMaskStack(ImagePlus imp, int label)
        {
            int s = imp.Slice;
            for (int z = 0; z < s; z++)
            {
                for (int y = 0; y < imp.Height; y++)
                {
                    for (int x = 0; x < imp.Width; x++)
                    {
                        double v = imp.GetXYZ(x, y, z);
                        if ((int)v == label)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public static double GetDoubleUlp(double value) 
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            double nextValue = BitConverter.Int64BitsToDouble(bits + 1);
            double result = nextValue - value;
            return result;
        }
        public static double[] Convert2DToArray(double[][] data) 
        {
            double[] newData= new double[data.Length*data[0].Length];
            for (int i = 0; i < data.Length; i++) 
            {
                for (int j = 0; j < data[0].Length; j++) 
                {
                    newData[i* data[0].Length + j]= data[i][j];
                }
            }
            return newData;
        }
        public static double Min(double[] data) 
        {
            double min = double.MaxValue;
            for (int i = 0; i < data.Length; i++) 
            {
                if (data[i] < min) 
                {
                    min = data[i];
                }
            }
            return min;
        }
        public static int[] GetHistogram(double[] discretisedVoxels)
        {
            if (discretisedVoxels == null || discretisedVoxels.Length == 0)
            {
                return null;
            }
            double[] voxels = discretisedVoxels;
            int size = voxels.Length;
            double max = Statistics.Maximum(voxels);
            // Generate histogram
            int nBins = (int)max;
            /*
             * histgram must start from 0
             */
            int[] histogram = new int[nBins];
            //init
            for (int i = 0; i < histogram.Length; i++) histogram[i] = 0;
            for (int i = 0; i < size; i++) histogram[(int)voxels[i] - 1]++;
            return histogram;
        }
        public static Dictionary<String, double[]> GetRoiBoundingBoxInfo(ImagePlus mask, int label)
        {
            double[] xMinMax = new double[] { Double.MaxValue, 0 };
            double[] yMinMax = new double[] { Double.MaxValue, 0 };
            double[] zMinMax = new double[] { Double.MaxValue, 0 };
            int w = mask.Width;
            int h = mask.Height;
            int s = mask.Slice;
            for (int z = 0; z < s; z++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int v = (int)mask.GetXYZ(x,y,z);
                        if (v == label)
                        {
                            if (xMinMax[0] > x)
                            {
                                xMinMax[0] = (double)x;
                            }
                            if (xMinMax[1] < x)
                            {
                                xMinMax[1] = (double)x;
                            }
                            if (yMinMax[0] > y)
                            {
                                yMinMax[0] = (double)y;
                            }
                            if (yMinMax[1] < y)
                            {
                                yMinMax[1] = (double)y;
                            }
                            if (zMinMax[0] > z)
                            {
                                zMinMax[0] = (double)z;
                            }
                            if (zMinMax[1] < z)
                            {
                                zMinMax[1] = (double)z;
                            }
                        }
                    }
                }
            }
            Dictionary<String, double[]> xyz = new Dictionary<string, double[]>();
            xyz.Add("x", xMinMax);
            xyz.Add("y", yMinMax);
            xyz.Add("z", zMinMax);
            return xyz;
        }
        public static Dictionary<int, int[]> BuildAngles()
        {
            // angle 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26
            // dim z -1 0 1 -1 0 1 -1 0 1 -1 0 1 -1 0 1 -1 0 1 -1 0 1 -1 0 1 -1 0 1
            // dim y -1 -1 -1 0 0 0 1 1 1 -1 -1 -1 0 0 0 1 1 1 -1 -1 -1 0 0 0 1 1 1
            // dim x -1 -1 -1 -1 -1 -1 -1 -1 -1 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 1
            Dictionary<int, int[]> angles = new Dictionary<int, int[]>();
            angles.Add(0, new int[] { -1, -1, -1 });// z,y,x
            angles.Add(1, new int[] { 0, -1, -1 });
            angles.Add(2, new int[] { 1, -1, -1 });
            angles.Add(3, new int[] { -1, 0, -1 });
            angles.Add(4, new int[] { 0, 0, -1 });
            angles.Add(5, new int[] { 1, 0, -1 });
            angles.Add(6, new int[] { -1, 1, -1 });
            angles.Add(7, new int[] { 0, 1, -1 });
            angles.Add(8, new int[] { 1, 1, -1 });
            angles.Add(9, new int[] { -1, -1, 0 });
            angles.Add(10, new int[] { 0, -1, 0 });
            angles.Add(11, new int[] { 1, -1, 0 });
            angles.Add(12, new int[] { -1, 0, 0 });
            angles.Add(13, new int[] { 0, 0, 0 });// own voxel
            angles.Add(14, new int[] { 1, 0, 0 });
            angles.Add(15, new int[] { -1, 1, 0 });
            angles.Add(16, new int[] { 0, 1, 0 });// 90 degree
            angles.Add(17, new int[] { 1, 1, 0 });
            angles.Add(18, new int[] { -1, -1, 1 });
            angles.Add(19, new int[] { 0, -1, 1 });// 135 degree
            angles.Add(20, new int[] { 1, -1, 1 });
            angles.Add(21, new int[] { -1, 0, 1 });
            angles.Add(22, new int[] { 0, 0, 1 });// 0 degree
            angles.Add(23, new int[] { 1, 0, 1 });
            angles.Add(24, new int[] { -1, 1, 1 });
            angles.Add(25, new int[] { 0, 1, 1 });// 45 degree
            angles.Add(26, new int[] { 1, 1, 1 });
            return angles;
        }

        public static Dictionary<int, int[]> BulidAnglesFor2D()
        {
            Dictionary<int, int[]> angles = new Dictionary<int, int[]>();
            angles.Add(22, new int[] { 0, 0, 1 });//0
            angles.Add(25, new int[] { 0, 1, 1 });//45
            angles.Add(16, new int[] { 0, 1, 0 });//90
            angles.Add(19, new int[] { 0, -1, 1 });//135
            return angles;
        }

      
    }
}
