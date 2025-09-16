using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Render;
using Radiomics.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.ImageProcess
{
    public class ImagePlus
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Slice { get; set; }
        public int BitsAllocated { get; set; }
        public double[][] Stack { get; set; }

        public double PixelWidth { get; set; }
        public double PixelHeight { get; set; }
        public double PixelDepth { get; set; }
        public ImagePlus(int w, int h, int s)
        {
            Width = w;
            Height = h;
            Slice = s;
            Stack = new double[s][];
            for (int i = 0; i < s; i++)
            {
                Stack[i] = new double[w * h];
            }
        }

        public ImagePlus(List<DicomDataset> ds)
        {
            DicomPixelData pixelData = DicomPixelData.Create(ds[0]);
            if (pixelData.SamplesPerPixel > 1)
            {
                throw new CustomException(Errors.NotGrayImage);
            }
            double[] values = ds[0].GetValues<double>(DicomTag.PixelSpacing);
            PixelWidth = values[0];
            PixelHeight = values[1];
            PixelDepth = ds[0].GetValueOrDefault<double>(DicomTag.SliceThickness, 0, 0);
            Width = pixelData.Width;
            Height = pixelData.Height;
            Slice = ds.Count;
            BitsAllocated = pixelData.BitsAllocated;
            Stack = new double[Slice][];
            for (int s = 0; s < Slice; s++)
            {
                Stack[s] = new double[Width * Height];
                pixelData = DicomPixelData.Create(ds[s]);
                var pixelDataFactory = PixelDataFactory.Create(pixelData, 0); // returns IPixelData type

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Stack[s][i * Width + j] = pixelDataFactory.GetPixel(j, i);
                    }
                }
            }
        }
        public double GetXYZ(int x, int y, int z)
        {
            return Stack[z][y * Width + x];
        }

        public ImagePlus Copy()
        {
            ImagePlus imagePlus = new ImagePlus(Width, Height, Slice);
            imagePlus.BitsAllocated = BitsAllocated;
            imagePlus.PixelHeight = PixelHeight;
            imagePlus.PixelWidth = PixelWidth;
            imagePlus.PixelDepth = PixelDepth;
            imagePlus.BitsAllocated = BitsAllocated;
            for (int s = 0; s < Slice; s++)
            {
                Array.Copy(Stack[s], imagePlus.Stack[s], Stack[s].Length);
            }
            return imagePlus;
        }
        public void SetXYZ(int x, int y, int z, double val)
        {
            Stack[z][y * Width + x] = val;
        }
        public double[] Resize2D(int dstWidth, int dstHeight, int z, CaculateParams caculateParams)
        {
            if (Width == dstWidth && Height == dstHeight)
            {
                double[] data = new double[Width * Height];
                Array.Copy(Stack[z], data, data.Length);
                return data;
            }
            else if ((Width == 1 || Height == 1) && caculateParams.Interpolation2D != 0)
            {
                return ResizeLinearly(dstWidth, dstHeight, z);
            }
            else
            {
                double srcCenterX = Width / 2.0D;
                double srcCenterY = Height / 2.0D;
                double dstCenterX = dstWidth / 2.0D;
                double dstCenterY = dstHeight / 2.0D;
                double xScale = dstWidth / (double)Width;
                double yScale = dstHeight / (double)Height;
                if (caculateParams.Interpolation2D != 0)
                {
                    if (dstWidth != Width)
                    {
                        dstCenterX += xScale / 4.0D;
                    }

                    if (dstHeight != Height)
                    {
                        dstCenterY += yScale / 4.0D;
                    }
                }

                double[] pixels2 = new double[dstHeight * dstWidth];
                double xs;
                double ys;
                if (caculateParams.Interpolation2D == 2)
                {
                    for (int y = 0; y <= dstHeight - 1; ++y)
                    {
                        ys = (y - dstCenterY) / yScale + srcCenterY;
                        int index = y * dstWidth;

                        for (int x = 0; x <= dstWidth - 1; ++x)
                        {
                            xs = (x - dstCenterX) / xScale + srcCenterX;
                            pixels2[index++] = (float)GetBicubicInterpolatedPixel(xs, ys, z, caculateParams);
                        }
                    }
                }
                else
                {
                    double xlimit = Width - 1.0D;
                    double xlimit2 = Width - 1.001D;
                    double ylimit = Height - 1.0D;
                    double ylimit2 = Height - 1.001D;

                    for (int y = 0; y <= dstHeight - 1; ++y)
                    {
                        ys = (y - dstCenterY) / yScale + srcCenterY;
                        if (caculateParams.Interpolation2D == 1)
                        {
                            if (ys < 0.0D)
                            {
                                ys = 0.0D;
                            }

                            if (ys >= ylimit)
                            {
                                ys = ylimit2;
                            }
                        }

                        int index1 = Width * (int)ys;
                        int index2 = y * dstWidth;

                        for (int x = 0; x <= dstWidth - 1; ++x)
                        {
                            xs = (x - dstCenterX) / xScale + srcCenterX;
                            if (caculateParams.Interpolation2D == 1)
                            {
                                if (xs < 0.0D)
                                {
                                    xs = 0.0D;
                                }

                                if (xs >= xlimit)
                                {
                                    xs = xlimit2;
                                }

                                pixels2[index2++] = GetInterpolatedPixel(xs, ys, Stack[z]);
                            }
                            else
                            {
                                pixels2[index2++] = Stack[z][index1 + (int)xs];
                            }
                        }
                    }
                }
                return pixels2;
            }
        }
        public double GetBicubicInterpolatedPixel(double x0, double y0, int z, CaculateParams caculateParams)
        {
            int u0 = (int)Math.Floor(x0);
            int v0 = (int)Math.Floor(y0);
            if (u0 > 0 && u0 < Width - 2 && v0 > 0 && v0 < Height - 2)
            {
                double q = 0.0D;

                for (int j = 0; j <= 3; ++j)
                {
                    int v = v0 - 1 + j;
                    double p = 0.0D;

                    for (int i = 0; i <= 3; ++i)
                    {
                        int u = u0 - 1 + i;
                        p += (double)GetXYZ(u, v, z) * cubic(x0 - u);
                    }

                    q += p * cubic(y0 - v);
                }

                return q;
            }
            else
            {
                return GetBilinearInterpolatedPixel(x0, y0, z, caculateParams);
            }
        }
        public static double cubic(double x)
        {
            if (x < 0.0D)
            {
                x = -x;
            }

            double z = 0.0D;
            if (x < 1.0D)
            {
                z = x * x * (x * 1.5D + -2.5D) + 1.0D;
            }
            else if (x < 2.0D)
            {
                z = -0.5D * x * x * x + 2.5D * x * x - 4.0D * x + 2.0D;
            }

            return z;
        }
        private double GetInterpolatedPixel(double x, double y, double[] pixels)
        {
            int xbase = (int)x;
            int ybase = (int)y;
            double xFraction = x - xbase;
            double yFraction = y - ybase;
            int offset = ybase * Width + xbase;
            double lowerLeft = pixels[offset];
            double lowerRight = pixels[offset + 1];
            double upperRight = pixels[offset + Width + 1];
            double upperLeft = pixels[offset + Width];
            double upperAverage;
            if (double.IsNaN(upperLeft) && xFraction >= 0.5D)
            {
                upperAverage = upperRight;
            }
            else if (double.IsNaN(upperRight) && xFraction < 0.5D)
            {
                upperAverage = upperLeft;
            }
            else
            {
                upperAverage = upperLeft + xFraction * (upperRight - upperLeft);
            }

            double lowerAverage;
            if (double.IsNaN(lowerLeft) && xFraction >= 0.5D)
            {
                lowerAverage = lowerRight;
            }
            else if (double.IsNaN(lowerRight) && xFraction < 0.5D)
            {
                lowerAverage = lowerLeft;
            }
            else
            {
                lowerAverage = lowerLeft + xFraction * (lowerRight - lowerLeft);
            }

            if (double.IsNaN(lowerAverage) && yFraction >= 0.5D)
            {
                return upperAverage;
            }
            else
            {
                return double.IsNaN(upperAverage) && yFraction < 0.5D ? lowerAverage : lowerAverage + yFraction * (upperAverage - lowerAverage);
            }
        }
        private double GetBilinearInterpolatedPixel(double x, double y, int z, CaculateParams caculateParams)
        {
            if (x >= -1.0D && x < Width && y >= -1.0D && y < Height)
            {
                int method = caculateParams.Interpolation2D;
                caculateParams.Interpolation2D = 1;
                double value = GetInterpolatedPixel(x, y, z, caculateParams);
                caculateParams.Interpolation2D = method;
                return value;
            }
            else
            {
                return Stack[z].Min();
            }
        }
        public double GetInterpolatedPixel(double x, double y, int z, CaculateParams caculateParams)
        {
            if (caculateParams.Interpolation2D == 2)
            {
                return GetBicubicInterpolatedPixel(x, y, z, caculateParams);
            }
            else
            {
                if (x < 0.0D)
                {
                    x = 0.0D;
                }

                if (x >= Width - 1.0D)
                {
                    x = Width - 1.001D;
                }

                if (y < 0.0D)
                {
                    y = 0.0D;
                }

                if (y >= Height - 1.0D)
                {
                    y = Height - 1.001D;
                }

                return GetInterpolatedPixel(x, y, Stack[z]);
            }
        }
        protected double[] ResizeLinearly(int width2, int height2, int z)
        {
            bool rotate = Width == 1;
            int width1;
            if (rotate)
            {
                width1 = width2;
                width2 = height2;
                height2 = width1;
            }
            width1 = Width;
            double scale = (width1 - 1) / (double)(width2 - 1);
            double[] data2 = new double[width2];
            int y;
            for (y = 0; y < width2; ++y)
            {
                int x1 = (int)(y * scale);
                int x2 = x1 + 1;
                if (x2 == width1)
                {
                    x2 = width1 - 1;
                }

                double fraction = y * scale - x1;
                data2[y] = (1.0D - fraction) * Stack[z][x1] + fraction * Stack[z][x2];
            }
            return data2;
        }
    }
}
