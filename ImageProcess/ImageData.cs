using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.ImageProcess
{
    public class ImageData
    {
        private ImagePlus imageOrig;
        /**
	 * Mofified image.
	 */
        private ImagePlus imageModif;

        /**
	 * Represents width of the image. (x)
	 */
        private int width;

        /**
         * Represents height of the image. (y)
         */
        private int height;

        /**
         * Number of slices in the images.
         */
        private int nSlices;
        //分解级数
        private int scale=1;
        /**
         * Number of pixels in the image. 
         */
        private int imageSize;
        /**
	 * 3D array of data representing transform coefficients (it is afterwards converted to <c>imageWave</c>).
	 */
        private double[,,] transformedData;

        private double[,,] imageData;
        /**
	 * Selected Wavelet Filter
	 */
        private WaveletFilters waveletFilter;
        public ImageData(ImagePlus image) 
        {
            this.imageOrig = image;
            this.width = image.Width;
            this.height = image.Height;
            this.nSlices = image.Slice;
            imageSize = width * height;
            imageData = new double[nSlices, height, width];
            transformedData = new double[nSlices, height, width];
            this.waveletFilter = new WaveletFilters();

        }
        public void FwdTransform(int z)
        {
            FWT2D(z);               // FWT2D from imageData into transformedData by slices
            //prepareStretchWC(z);    // intensity stretch preparation of transformedData slices		
        }
        public void InvTransform(int z)
        {

            //dataToWave(z);          // stretched transformedData into imageWave for visualization
            IWT2D(z);               // IWT2D from transformedData into imageData by slices
        }
        /**
     * Algorithm for 2D Fast Wavelet Transform. It performs 1D FWT algorithm for all rows and then all columns.
     * @param z - slice
     */
        private void FWT2D(int z)
        {
            double[][] tempData = new double[height][]; //temporary data buffer for just this occurence, not needed after transform
            for (int y = 0; y < height; y++)
            {
                tempData[y] = new double[width];
                for (int x = 0; x < width; x++)
                    tempData[y][x] = imageOrig.GetXYZ(x,y,z);
            }
            double[] row;
            double[] col;

            for (int k = 0; k < scale; k++)
            {
                int lev = 1 << k;

                int levCols = height / lev;
                int levRows = width / lev;

                row = new double[levCols];
                for (int x = 0; x < levRows; x++)
                {
                    for (int y = 0; y < row.Length; y++)
                        row[y] = tempData[y][x];

                    FWT(row);

                    for (int y = 0; y < row.Length; y++)
                        tempData[y][x] = row[y];
                }

                col = new double[levRows];
                for (int y = 0; y < levCols; y++)
                {
                    for (int x = 0; x < col.Length; x++)
                        col[x] = tempData[y][x];

                    FWT(col);

                    for (int x = 0; x < col.Length; x++)
                        tempData[y][x] = col[x];
                }
            }

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    transformedData[z, y, x] = tempData[y][x];
        }
        /**
     * Changes scale (level of detail).
     * @param scale - Scale to be changed.
     * @return 0 if the scale cannot be used, otherwise returns maximal scale.
     */
        public int ChangeScale(int scale)
        {
            int maxScale = (int)(Math.Log10(width < height ? width : height) / Math.Log10(2));

            if (scale < 1 || scale > maxScale)
                return 0;   //invalid scale
            else
            {
               this.scale = scale;
               return maxScale;
            }
        }
        /**
	 * Algorithm for 1D Fast Wavelet Transform. It is used in <c>FWT2D</c> method.
	 * @param data - 1D input data. The result of the transform is stored here afterwards.
	 */
        private void FWT(double[] data)
        {
            int dataLen = data.Length;
            int kernelLen = waveletFilter.dlf.Length;
            int mid = dataLen / 2;
            int midKernel = kernelLen / 2;
            double sumL, sumH;
            int pad = dataLen * 30;   //in order not to get index < 0 //TODO for smaller images a greater value might be needed, or some walkaround

            double[] approx = new double[dataLen];
            double[] detail = new double[dataLen];

            for (int i = 0; i < dataLen; i++)
            {
                sumL = 0.0;
                sumH = 0.0;
                for (int j = 0; j < kernelLen; j++)
                {
                    sumL += data[(i - midKernel + j + pad) % dataLen] * waveletFilter.dlf[j];
                    sumH += data[(i - midKernel + j + pad) % dataLen] * waveletFilter.dhf[j];
                }
                approx[i] = sumL;
                detail[i] = sumH;
            }

            int k;
            for (int i = 0; i < mid; i++)
            {
                k = i * 2;
                data[i] = approx[k];
                data[mid + i] = detail[k];
            }
        }
        /**
     * Algorithm for 1D Inverse Fast Wavelet Transform. It is used in <c>IWT2D</c> method.
     * @param data - 1D input data. The result of the transform is stored here afterwards.
     */
        private void IWT(double[] data)
        {
            int dataLen = data.Length;
            int kernelLen = waveletFilter.rlf.Length;
            int mid = dataLen / 2;
            int midKernel = kernelLen / 2;
            int k;
            double sumL, sumH;
            int pad = dataLen * 20;

            double[] approxUp = new double[dataLen];
            double[] detailUp = new double[dataLen];
            double[] temp = new double[dataLen];

            for (int i = 0; i < mid; i++)
            {
                k = i * 2;
                approxUp[k] = data[i];
                approxUp[k + 1] = 0.0;
                detailUp[k] = data[mid + i];
                detailUp[k + 1] = 0.0;
            }

            for (int i = 0; i < dataLen; i++)
            {
                sumL = 0;
                sumH = 0;
                for (int j = 0; j < kernelLen; j++)
                {
                    sumL += approxUp[(i - midKernel + j + pad) % dataLen] * waveletFilter.rlf[j];
                    sumH += detailUp[(i - midKernel + j + pad) % dataLen] * waveletFilter.rhf[j];
                }
                temp[i] = sumL + sumH;
            }

            for (int i = 0; i < dataLen - 1; i++)
            {
                data[i] = temp[i + 1];
            }
            data[dataLen - 1] = temp[0];
        }

        /**
         * Algorithm for 2D Inverse Fast Wavelet Transform. It performs 1D IWT algorithm for all columns and then all rows.
         * @param z - slice
         */
        private void IWT2D(int z)
        {
            double[,] tempData = new double[height,width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    tempData[y, x] = transformedData[z, y, x];

            double[] col;
            double[] row;

            for (int k = scale - 1; k >= 0; k--)
            {
                int lev = 1 << k;

                int levCols = height / lev;
                int levRows = width / lev;

                col = new double[levRows];

                for (int y = 0; y < levCols; y++)
                {
                    for (int x = 0; x < col.Length; x++)
                        col[x] = tempData[y, x];

                    IWT(col);

                    for (int x = 0; x < col.Length; x++)
                        tempData[y, x] = col[x];
                }

                row = new double[levCols];
                for (int x = 0; x < levRows; x++)
                {
                    for (int y = 0; y < row.Length; y++)
                        row[y] = tempData[y, x];

                    IWT(row);

                    for (int y = 0; y < row.Length; y++)
                        tempData[y, x] = row[y];
                }
            }

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    imageData[z, y, x] = tempData[y, x];
        }
        public ImagePlus GetFilterImage() 
        {
            ImagePlus newImage = imageOrig.Copy();
            for (int z = 0; z < nSlices; z++) 
            {
                for (int y = 0; y < height; y++) 
                {
                    for (int x = 0; x < width; x++) 
                    {
                        newImage.SetXYZ(x, y, z, imageData[z,y,x]);
                    }
                }
            }
            return newImage;
        }
        public void SetWaveletFilter(string filterName)
        {
            switch (filterName)
            {
                // Haar
                case "haar":
                    waveletFilter.SetFilter("haar1.flt");
                    break;

                // Daubechies
                case "db1":
                    waveletFilter.SetFilter("daub1.flt");
                    break;
                case "db 2":
                    waveletFilter.SetFilter("daub2.flt");
                    break;
                case "db3":
                    waveletFilter.SetFilter("daub3.flt");
                    break;
                case "db4":
                    waveletFilter.SetFilter("daub4.flt");
                    break;
                case "db5":
                    waveletFilter.SetFilter("daub5.flt");
                    break;
                case "db6":
                    waveletFilter.SetFilter("daub6.flt");
                    break;
                case "db7":
                    waveletFilter.SetFilter("daub7.flt");
                    break;
                case "db8":
                    waveletFilter.SetFilter("daub8.flt");
                    break;
                case "db9":
                    waveletFilter.SetFilter("daub9.flt");
                    break;
                case "db10":
                    waveletFilter.SetFilter("daub10.flt");
                    break;
                case "db11":
                    waveletFilter.SetFilter("daub11.flt");
                    break;
                case "db12":
                    waveletFilter.SetFilter("daub12.flt");
                    break;
                case "db13":
                    waveletFilter.SetFilter("daub13.flt");
                    break;
                case "db14":
                    waveletFilter.SetFilter("daub14.flt");
                    break;
                case "db15":
                    waveletFilter.SetFilter("daub15.flt");
                    break;
                case "db16":
                    waveletFilter.SetFilter("daub16.flt");
                    break;
                case "db17":
                    waveletFilter.SetFilter("daub17.flt");
                    break;
                case "db18":
                    waveletFilter.SetFilter("daub18.flt");
                    break;
                case "db19":
                    waveletFilter.SetFilter("daub19.flt");
                    break;
                case "db20":
                    waveletFilter.SetFilter("daub20.flt");
                    break;

                // Symlets
                case "sym2":
                    waveletFilter.SetFilter("sym2.flt");
                    break;
                case "sym3":
                    waveletFilter.SetFilter("sym3.flt");
                    break;
                case "sym4":
                    waveletFilter.SetFilter("sym4.flt");
                    break;
                case "sym5":
                    waveletFilter.SetFilter("sym5.flt");
                    break;
                case "sym6":
                    waveletFilter.SetFilter("sym6.flt");
                    break;
                case "sym7":
                    waveletFilter.SetFilter("sym7.flt");
                    break;
                case "sym8":
                    waveletFilter.SetFilter("sym8.flt");
                    break;
                case "sym9":
                    waveletFilter.SetFilter("sym9.flt");
                    break;
                case "sym10":
                    waveletFilter.SetFilter("sym10.flt");
                    break;
                case "sym11":
                    waveletFilter.SetFilter("sym11.flt");
                    break;
                case "sym12":
                    waveletFilter.SetFilter("sym12.flt");
                    break;
                case "sym13":
                    waveletFilter.SetFilter("sym13.flt");
                    break;
                case "sym14":
                    waveletFilter.SetFilter("sym14.flt");
                    break;
                case "sym15":
                    waveletFilter.SetFilter("sym15.flt");
                    break;
                case "sym16":
                    waveletFilter.SetFilter("sym16.flt");
                    break;
                case "sym17":
                    waveletFilter.SetFilter("sym17.flt");
                    break;
                case "sym18":
                    waveletFilter.SetFilter("sym18.flt");
                    break;
                case "sym19":
                    waveletFilter.SetFilter("sym19.flt");
                    break;
                case "sym20":
                    waveletFilter.SetFilter("sym20.flt");
                    break;

                // Coiflets
                case "coif1":
                    waveletFilter.SetFilter("coif1.flt");
                    break;
                case "coif2":
                    waveletFilter.SetFilter("coif2.flt");
                    break;
                case "coif3":
                    waveletFilter.SetFilter("coif3.flt");
                    break;
                case "coif4":
                    waveletFilter.SetFilter("coif4.flt");
                    break;
                case "coif5":
                    waveletFilter.SetFilter("coif5.flt");
                    break;

                // Biorthogonal
                case "bior1":
                    waveletFilter.SetFilter("biortho1.flt");
                    break;
                case "bior2":
                    waveletFilter.SetFilter("biortho2.flt");
                    break;
                case "bior3":
                    waveletFilter.SetFilter("biortho3.flt");
                    break;
                case "bior4":
                    waveletFilter.SetFilter("biortho4.flt");
                    break;
                case "bior5":
                    waveletFilter.SetFilter("biortho5.flt");
                    break;
                case "bior6":
                    waveletFilter.SetFilter("biortho6.flt");
                    break;
                case "bior7":
                    waveletFilter.SetFilter("biortho7.flt");
                    break;
                case "bior8":
                    waveletFilter.SetFilter("biortho8.flt");
                    break;
                case "bior9":
                    waveletFilter.SetFilter("biortho9.flt");
                    break;
                case "bior10":
                    waveletFilter.SetFilter("biortho10.flt");
                    break;
                case "bior11":
                    waveletFilter.SetFilter("biortho11.flt");
                    break;
                case "bior12":
                    waveletFilter.SetFilter("biortho12.flt");
                    break;
                case "bior13":
                    waveletFilter.SetFilter("biortho13.flt");
                    break;
                case "bior14":
                    waveletFilter.SetFilter("biortho14.flt");
                    break;
                case "bior15":
                    waveletFilter.SetFilter("biortho15.flt");
                    break;

                // Reverse Biorthogonal
                case "rbior1":
                    waveletFilter.SetFilter("revbiortho1.flt");
                    break;
                case "rbior2":
                    waveletFilter.SetFilter("revbiortho2.flt");
                    break;
                case "rbior3":
                    waveletFilter.SetFilter("revbiortho3.flt");
                    break;
                case "rbior4":
                    waveletFilter.SetFilter("revbiortho4.flt");
                    break;
                case "rbior5":
                    waveletFilter.SetFilter("revbiortho5.flt");
                    break;
                case "rbior6":
                    waveletFilter.SetFilter("revbiortho6.flt");
                    break;
                case "rbior7":
                    waveletFilter.SetFilter("revbiortho7.flt");
                    break;
                case "rbior8":
                    waveletFilter.SetFilter("revbiortho8.flt");
                    break;
                case "rbior9":
                    waveletFilter.SetFilter("revbiortho9.flt");
                    break;
                case "rbior10":
                    waveletFilter.SetFilter("revbiortho10.flt");
                    break;
                case "rbior11":
                    waveletFilter.SetFilter("revbiortho11.flt");
                    break;
                case "rbior12":
                    waveletFilter.SetFilter("revbiortho12.flt");
                    break;
                case "rbior13":
                    waveletFilter.SetFilter("revbiortho13.flt");
                    break;
                case "rbior14":
                    waveletFilter.SetFilter("revbiortho14.flt");
                    break;
                case "rbior15":
                    waveletFilter.SetFilter("revbiortho15.flt");
                    break;

                // Discrete Meyer 1
                case "meyer1":
                    waveletFilter.SetFilter("meyer1.flt");
                    break;

                default:
                    waveletFilter.SetFilter("haar1.flt");
                    break;
            }
            transposeFilters();
        }

        /**
         * Transpose selected filters so that they can be used for convolution.
         */
        private void transposeFilters()
        {
            int len = waveletFilter.dlf.Length;
            double[] dlfT = new double[len];
            double[] dhfT = new double[len];
            double[] rlfT = new double[len];
            double[] rhfT = new double[len];
            for (int i = 0; i < waveletFilter.dlf.Length; i++) //transposition
            {
                dlfT[i] = waveletFilter.dlf[len - i - 1];
                dhfT[i] = waveletFilter.dhf[len - i - 1];
                rlfT[i] = waveletFilter.rlf[len - i - 1];
                rhfT[i] = waveletFilter.rhf[len - i - 1];
            }
            waveletFilter.dlf = dlfT;
            waveletFilter.dhf = dhfT;
            waveletFilter.rlf = rlfT;
            waveletFilter.rhf = rhfT;
        }
    }
}
