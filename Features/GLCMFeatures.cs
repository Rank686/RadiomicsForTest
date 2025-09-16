using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Radiomics.Net.ImageProcess;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Features
{
    public class GLCMFeatures
    {
        ImagePlus orgImg;
        ImagePlus discImg;// discretised
        ImagePlus orgMask;
        int w;
        int h;
        int s;
        //axis aligned bb
        Dictionary<String, double[]> aabb;

        int label;
        Dictionary<int, double[][]> glcm_raw;// angle_id and glcm at it angle.
        Dictionary<int, double[][]> glcm;// angle_id and normalized glcm at it angle.
        bool symmetry = true;// always true;
        bool normalization = true;// always true;
        int nBins;// 1 to N
        int delta = 1;

        Dictionary<int, Dictionary<String, Object>> coeffs;// angle_od and coefficients of it angle.
        public static readonly String Px = "Px";
        public static readonly String Py = "Py";

        double eps = Utils.GetDoubleUlp(1.0);
        readonly String[] weightingMethods = new String[] { "no_weighting", "manhattan", "euclidian", "infinity" };
        String weightingMethod = null;
        /**
	 * delta: distance between i and j. 1 is default.\n angle: if 2d, 0, 45, 90,
	 * 135, \n else if 3d, 13 angles(in symmetrical).
	 * weightingNorm:"manhattan""euclidian""infinity""no_weighting", no weighting is
	 * default.
	 * 
	 * @throws Exception
	 */
        public GLCMFeatures(ImagePlus img, ImagePlus mask,ImagePlus discImg, CaculateParams caculateParams)
        {
            this.label = (int)caculateParams.Label;
            this.nBins= caculateParams.NBins;
            this.delta = caculateParams.GLCMDelta;
            SetWeightingNorm(caculateParams.GLCMWeightingMethod);
            this.orgImg = img;
            this.orgMask = mask;
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

            w = orgImg.Width;
            h = orgImg.Height;
            s = orgImg.Slice;
            aabb = Utils.GetRoiBoundingBoxInfo(orgMask,label);
            CalcGLCM();

        }
        public void CalcGLCM()
        {
            glcm_raw = new Dictionary<int, double[][]>();// phi_id and it glcm
            Dictionary<int, int[]> angles = Utils.BuildAngles();
            List<int> angle_ids = angles.Keys.ToList();
            angle_ids.Sort();
            int num_of_angles = angle_ids.Count;
            if (symmetry)
            {
                /*
                 * only calculate about 13 angles (symmetrical )
                 */
                for (int a = 14; a < num_of_angles; a++)
                {
                    double[][] glcm_at_a = CalcGLCM2(a, angles[a]);
                    //GLCM matrices are weighted by weighting factor W and then normalized.
                    glcm_at_a = weighting(angles[a], glcm_at_a);
                    if (glcm_at_a != null)
                        glcm_raw.Add(a, glcm_at_a);
                }
            }
            else
            {
                for (int a = 0; a < num_of_angles; a++)
                {
                    if (a == 13)
                    {// skip own angle int[]{0,0,0}
                        continue;
                    }
                    double[][] glcm_at_a = CalcGLCM2(a, angles[a]);
                    //GLCM matrices are weighted by weighting factor W and then normalized.
                    glcm_at_a = weighting(angles[a], glcm_at_a);
                    if (glcm_at_a != null)
                        glcm_raw.Add(a, glcm_at_a);
                }
            }
            Normalize(glcm_raw);
        }
        public double[][] CalcGLCM2(int angleID, int[] angle)
        {

            ImagePlus img = discImg;
            ImagePlus mask = orgMask;

            if (glcm_raw == null)
            {
                glcm_raw = new Dictionary<int, double[][]>();
            }
            if (delta < 1)
            {
                delta = 1;
            }

            double[][] glcm_at_angle = new double[nBins][];
            // init
            for (int y = 0; y < nBins; y++)
            {
                glcm_at_angle[y] = new double[nBins];
                for (int x = 0; x < nBins; x++)
                {
                    glcm_at_angle[y][x] = 0d;
                }
            }

            int offsetX = angle[2] * delta;
            int offsetY = angle[1] * delta * -1;//adjust vector direction and coordinate direction in Y axis.
            int offsetZ = angle[0] * delta;

            int xMin = (int)aabb["x"][0];
            int xMax = (int)aabb["x"][1];
            int yMin = (int)aabb["y"][0];
            int yMax = (int)aabb["y"][1];
            int zMin = (int)aabb["z"][0];
            int zMax = (int)aabb["z"][1];

            for (int z = zMin; z <= zMax; z++)
            {
                //searching slice
                int dz = z + offsetZ;
                for (int y = yMin; y <= yMax; y++)
                {
                    int dy = y + offsetY;
                    for (int x = xMin; x <= xMax; x++)
                    {
                        int dx = x + offsetX;
                        if ((dx >= 0 && dx < w) && (dy >= 0 && dy < h) && (dz >= 0 && dz < s))
                        {
                            int lbli = (int)mask.GetXYZ(x,y,z);
                            if (lbli == this.label)
                            {
                                /*
                                 * int value, because pixels were discretised.
                                 */
                                int vi = (int)img.GetXYZ(x, y, z);
                                int lblj = (int)mask.GetXYZ(dx, dy, dz);
                                if (lblj == this.label)
                                {
                                    int vj = (int)img.GetXYZ(dx, dy, dz);
                                    // discretised pixels is 1 to nBins
                                    glcm_at_angle[vi - 1][vj - 1]++;
                                    if (symmetry)
                                    {
                                        glcm_at_angle[vj - 1][vi - 1]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // null validate
            /*
             * if glcm matrix (at specified angle) values are all zero, return null.
             */
            bool return_null = true;
            for (int y = 0; y < nBins; y++)
            {
                for (int x = 0; x < nBins; x++)
                {
                    if (glcm_at_angle[y][x] > 0d)
                    {
                        return_null = false;
                    }
                }
            }
            if (return_null)
            {
                return null;
            }
            //Matrix<double> matrix = Matrix<double>.Build.Dense(nBins, nBins, Utils.Convert2DToArray(glcm_at_angle));
            //matrix = matrix + matrix.Transpose();
            //return matrix.ToRowArrays();
            return glcm_at_angle;
        }
        /**
	 * "manhattan"
	 * "euclidian" //default
	 * "infinity" | Chebyshev distance
	 * 
	 * @param angleVector
	 * @param glcm_raw
	 * @return weighted glcm_raw
	 */
        public double[][] weighting(int[] angleVector, double[][] glcm_raw)
        {
            double dx = this.w * angleVector[2];
            double dy = this.h * angleVector[1];
            double dz = this.s * angleVector[0];
            double distance = 1d;
            if (this.weightingMethod == null || this.weightingMethod.Equals("no_weighting"))
            {
                return glcm_raw;
            }
            else if (this.weightingMethod.Equals("manhattan"))
            {
                distance = dx + dy + dz;
            }
            else if (this.weightingMethod.Equals("euclidian"))
            {
                distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));
            }
            else if (this.weightingMethod.Equals("infinity"))
            {
                double max = 0d;
                foreach (double v  in new double[] { dx, dy, dz })
                {
                    if (max < v)
                    {
                        max = v;
                    }
                }
                distance = max;
            }
            else
            {
                return glcm_raw;
            }
            double w = Math.Exp(-1 * Math.Pow(distance, 2));
            double[][] weighted = new double[nBins][];
            for (int i = 0; i < nBins; i++)
            {
                weighted[i]=new double[nBins];
                for (int j = 0; j < nBins; j++)
                {
                    weighted[i][j] = glcm_raw[i][j] * w;
                }
            }
            return weighted;
        }
        public Dictionary<int, double[][]> Normalize(Dictionary<int, double[][]> glcm_raw)
        {
            glcm = new Dictionary<int, double[][]>();// final glcm set of each angles
            List<int> anglesKey = glcm_raw.Keys.ToList();
            anglesKey.Sort();

            /*
             * if do distance weighting, do first merge all angles, then calculate features.
             */
            foreach (int a in anglesKey)
            {
                double[][] glcm_raw_at_a = glcm_raw[a];
                // skip all zero matrix
                if (glcm_raw_at_a == null)
                {
                    glcm.Add(a, null);
                    continue;
                }
                double[][] norm_glcm_at_a = Normalize(glcm_raw_at_a);
                glcm.Add(a, norm_glcm_at_a);
            }

            CalculateCoefficients();

            return glcm;
        }
        public double[][] Normalize(double[][] glcm_raw)
        {
            double[][] norm_glcm = new double[nBins][];
            // init array
            for (int y = 0; y < nBins; y++)
            {
                norm_glcm[y] = new double[nBins];
                for (int x = 0; x < nBins; x++)
                {
                    norm_glcm[y][x] = 0d;
                }
            }
            double sum = 0d;
            for (int i = 0; i < nBins; i++)
            {
                for (int j = 0; j < nBins; j++)
                {
                    sum += glcm_raw[i][j];
                }
            }
            for (int i = 0; i < nBins; i++)
            {
                for (int j = 0; j < nBins; j++)
                {
                    norm_glcm[i][j] = glcm_raw[i][j] / sum;
                }
            }
            return norm_glcm;
        }
        //计算XY相关系数
        private void CalculateCoefficients()
        {
            coeffs = new Dictionary<int, Dictionary<String, object>>();
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    this.coeffs.Add(a, null);
                    continue;
                }
                Dictionary<String, object> coeffs_a = new Dictionary<string, object>();
                double[] px = new double[nBins];
                double[] py = new double[nBins];
                //p_{i+j}
                double[] pXAddY = new double[(nBins * 2) - 1];// 2 to nBins*2
                                                              //p_{i-j}
                double[] pXSubY = new double[nBins];
                double ux = 0.0;
                double uy = 0.0;
                double stdevx = 0.0;
                double stdevy = 0.0;

                // Px(i) the marginal row probabilities
                // Py(i) the marginal column probabilities
                // First, initialize the arrays to 0
                for (int i = 0; i < nBins; i++)
                {
                    px[i] = 0.0;
                    py[i] = 0.0;
                }

                for (int i = 0; i < nBins; i++)
                {
                    for (int j = 0; j < nBins; j++)
                    {
                        px[i] += glcm_a[i][j];// sum of the cols at row
                        py[i] += glcm_a[j][i];// sum of the rows at col
                    }
                }
                // calculate meanx and meany
                for (int i = 1; i <= nBins; i++)
                {
                    ux += (i * px[i - 1]);
                    uy += (i * py[i - 1]);
                }
                // calculate stdevx and stdevy
                for (int i = 1; i <= nBins; i++)
                {
                    stdevx += ((Math.Pow((i - ux), 2)) * px[i - 1]);
                    stdevy += ((Math.Pow((i - uy), 2)) * py[i - 1]);
                }
                stdevx = Math.Sqrt(stdevx);
                stdevy = Math.Sqrt(stdevy);

                int addK_max = nBins * 2;
                for (int k = 2; k <= addK_max; k++)
                {
                    for (int i = 1; i <= nBins; i++)
                    {
                        for (int j = 1; j <= nBins; j++)
                        {
                            if (k == (i + j))
                            {
                                pXAddY[k - 2] += glcm_a[i - 1][j - 1];
                            }
                        }
                    }
                }
                int subK_max = nBins;
                for (int k = 0; k < subK_max; k++)
                {
                    for (int i = 1; i <= nBins; i++)
                    {
                        for (int j = 1; j <= nBins; j++)
                        {
                            if (k == Math.Abs(i - j))
                            {
                                pXSubY[k] += glcm_a[i - 1][j - 1];
                            }
                        }
                    }
                }
                coeffs_a.Add("Px", px);
                coeffs_a.Add("Py", py);
                coeffs_a.Add("MeanX", ux);
                coeffs_a.Add("MeanY", uy);
                coeffs_a.Add("pXAddY", pXAddY);
                coeffs_a.Add("pXSubY", pXSubY);
                coeffs_a.Add("StdDevX", stdevx);
                coeffs_a.Add("StdDevY", stdevy);
                this.coeffs.Add(a, coeffs_a);
            }
        }
        private void SetWeightingNorm(String weightingMethod)
        {
            if (weightingMethod == null)
            {
                this.weightingMethod = null;// none weighting (ignore weighting).
                return;
            }
            foreach (String methodname in weightingMethods)
            {
                if (weightingMethod.Equals(methodname))
                {
                    this.weightingMethod = methodname;
                    return;
                }
            }
            this.weightingMethod = null;// no weighting (ignore weighting).
        }
        public double GetMaximumProbability()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                //for example, at force2D calculation only used 4 angles.
                if (glcm_a == null)
                {
                    continue;
                }
                double max_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        double v = glcm_a[i - 1][j - 1];
                        if (max_a < v)
                        {
                            max_a = v;
                        }
                    }
                }
                res_set[itr] = max_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetJointAverage()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                //for example, at force2D calculation only used 4 angles.
                if (glcm_a == null)
                {
                    continue;
                }
                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += glcm_a[i - 1][j - 1] * (double)(i);
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        // calculate the variance ("variance" in Walker 1995; "Sum of Squares: Variance"
        // in Haralick 1973)
        // also called Sum of Squares.
        public double GetSumSquares()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                //for example, at force2D calculation only used 4 angles.
                if (glcm_a == null)
                {
                    continue;
                }
                double myu_a = 0d;// joint average at angle
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        myu_a += glcm_a[i - 1][j - 1] * (double)(i);
                    }
                }

                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += Math.Pow(i - myu_a, 2) * glcm_a[i - 1][j - 1];
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // ===============================================================================================
        // calculate the entropy (Haralick et al., 1973; Walker, et al., 1995)
        public double GetJointEntropy()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                //for example, at force2D calculation only used 4 angles.
                if (glcm_a == null)
                {
                    continue;
                }
                double entropy = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            entropy = entropy - (glcm_a[i - 1][j - 1] * ((Math.Log(glcm_a[i - 1][j - 1] + eps)) / Math.Log(2.0)));
                        }
                    }
                }
                res_set[itr] = entropy;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetDifferenceAverage()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;
                for (int k = 0; k < nBins; k++)
                {
                    res_a += pXSubY[k] * k;
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetDifferenceVariance()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;
                double diff_avg = 0d;
                for (int k = 0; k < nBins; k++)
                {
                    diff_avg += pXSubY[k] * k;
                }
                for (int k = 0; k < nBins; k++)
                {
                    res_a += Math.Pow((k - diff_avg), 2) * pXSubY[k];
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetDifferenceEntropy()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;

                for (int k = 0; k < nBins; k++)
                {
                    res_a -= pXSubY[k] * (Math.Log(pXSubY[k] + eps) / Math.Log(2.0));
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetSumAverage()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXAddY = (double[])coeffs[a]["pXAddY"];
                double res_a = 0.0;
                for (int k = 2; k <= nBins * 2; k++)
                {
                    res_a += pXAddY[k - 2] * k;
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetSumVariance()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXAddY = (double[])coeffs[a]["pXAddY"];
                double myu = 0;
                for (int k = 2; k <= nBins * 2; k++)
                {
                    myu += pXAddY[k - 2] * k;
                }
                double res_a = 0.0;
                for (int k = 2; k <= nBins * 2; k++)
                {
                    res_a += Math.Pow(k - myu, 2) * pXAddY[k - 2];
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetSumEntropy()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXAddY = (double[])coeffs[a]["pXAddY"];
                double res_a = 0.0;
                for (int k = 2; k <= nBins * 2; k++)
                {
                    res_a += pXAddY[k - 2] * (Math.Log(pXAddY[k - 2] + eps) / Math.Log(2.0));
                }
                res_set[itr] = -1 * res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        // calculate the angular second moment (asm)
        /**
         * also known as 'energy' (formula 15.38, Bankman, 2009)
         * 
         * @return
         */
        public double GetAngular2ndMoment()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    continue;
                }
                double res_a = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            res_a += glcm_a[i - 1][j - 1] * glcm_a[i - 1][j - 1];
                        }
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }
        // (formula 15.39, Bankman, 2009) energy weighted by pixel value difference
        // same as Inertia.
        public double GetContrast()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    continue;
                }
                double contrast = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            contrast += Math.Pow(i - j, 2) * (glcm_a[i - 1][j - 1]);
                        }
                    }
                }
                res_set[itr] = contrast;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetDissimilarity()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    continue;
                }
                double res_a = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            res_a += Math.Abs(i - j) * (glcm_a[i - 1][j - 1]);
                        }
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetInverseDifference()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;
                for (int k = 0; k < nBins; k++)
                {
                    res_a += pXSubY[k] / (1.0 + k);
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetNormalisedInverseDifference()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;
                for (int k = 0; k < nBins; k++)
                {
                    res_a += pXSubY[k] / (1.0 + k / (double)nBins);
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetInverseDifferenceMoment()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    continue;
                }
                double res_a = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            res_a += glcm_a[i - 1][j - 1] / (1.0 + (Math.Pow(i - j, 2)));
                        }
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetNormalisedInverseDifferenceMoment()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                double[][] glcm_a = glcm[a];
                if (glcm_a == null)
                {
                    continue;
                }
                double res_a = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        if (glcm_a[i - 1][j - 1] != 0)
                        {
                            res_a += glcm_a[i - 1][j - 1] / (1.0 + (Math.Pow(i - j, 2) / Math.Pow(nBins, 2)));
                        }
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        public double GetInverseVariance()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (coeffs[a] == null)
                {
                    continue;
                }
                double[] pXSubY = (double[])coeffs[a]["pXSubY"];
                double res_a = 0.0;
                for (int k = 1; k < nBins; k++)
                { // 1 <= k <= Ng-1
                  //				if (!Double.isNaN(Double.valueOf(pXSubY[k])) && pXSubY[k] != 0) {
                    res_a += pXSubY[k] / (double)(k * k);
                    //				}
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        /**
         * calculate the correlation methods based on Haralick 1973 (and MatLab), Walker
         * 1995 are included below Haralick/Matlab result reported for correlation
         * currently; will give Walker as an option in the future
         */
        public double GetCorrelation()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double meanX = (double)coeffs[a]["MeanX"];
                double meanY = (double)coeffs[a]["MeanY"];
                double stdevX = (double)coeffs[a]["StdDevX"];
                double stdevY = (double)coeffs[a]["StdDevY"];
                double res_a = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += ((i - meanX) * (j - meanY)) * glcm_a[i - 1][j - 1];
                    }
                }
                res_set[itr] = (1d / (stdevX * stdevY)) * res_a;
                itr++;
            }
            return Statistics.Mean(res_set);

        }

        // =====================================================================================================
        public double GetAutocorrection()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += glcm_a[i - 1][j - 1] * (double)(i * j);
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetClusterTendency()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double meanX = (double)coeffs[a]["MeanX"];
                double meanY = (double)coeffs[a]["MeanY"];
                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += (Math.Pow((i + j - meanX - meanY), 2) * glcm_a[i - 1][j - 1]);
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        /**
         * Shade calculate the shade (Walker, et al., 1995; Connors, et al. 1984)
         * 
         * @return
         */
        public double GetClusterShade()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double meanX = (double)coeffs[a]["MeanX"];
                double meanY = (double)coeffs[a]["MeanY"];
                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += (Math.Pow((i + j - meanX - meanY), 3) * glcm_a[i - 1][j - 1]);
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // =====================================================================================================
        public double GetClusterProminence()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double meanX = (double)coeffs[a]["MeanX"];
                double meanY = (double)coeffs[a]["MeanY"];
                double res_a = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        res_a += (Math.Pow((i + j - meanX - meanY), 4) * glcm_a[i - 1][j - 1]);
                    }
                }
                res_set[itr] = res_a;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // ===============================================================================================
        // calculate the energy- same as Angular 2nd Moment-
        public double GetJointEnergy()
        {
            return GetAngular2ndMoment();
        }

        // ===============================================================================================
        public double GetInformationalMeasureOfCorrelation1()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double[] Px = (double[])coeffs[a]["Px"];
                double[] Py = (double[])coeffs[a]["Py"];
                double HXY = 0.0; // JointEntropy
                double HXY1 = 0.0;
                double HX = 0d;
                for (int i = 1; i <= nBins; i++)
                {
                    HX -= Px[i - 1] * ((Math.Log(Px[i - 1] + eps)) / Math.Log(2.0));
                    for (int j = 1; j <= nBins; j++)
                    {
                        HXY -= (glcm_a[i - 1][j - 1] * ((Math.Log(glcm_a[i - 1][j - 1] + eps)) / Math.Log(2.0)));
                        HXY1 -= (glcm_a[i - 1][j - 1] * (Math.Log((Px[i - 1] * Py[j - 1]) + eps) / Math.Log(2.0)));
                    }
                }
                res_set[itr] = (HXY - HXY1) / HX;
                itr++;
            }
            return Statistics.Mean(res_set);
        }

        // ===============================================================================================
        public double GetInformationalMeasureOfCorrelation2()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double[] Px = (double[])coeffs[a]["Px"];
                double[] Py = (double[])coeffs[a]["Py"];
                double HXY = 0.0; // JointEntropy
                double HXY2 = 0.0;
                for (int i = 1; i <= nBins; i++)
                {
                    for (int j = 1; j <= nBins; j++)
                    {
                        HXY = HXY - glcm_a[i - 1][j - 1] * (Math.Log((glcm_a[i - 1][j - 1]) + eps) / Math.Log(2.0));
                        HXY2 = HXY2 - Px[i - 1] * Py[j - 1] * (Math.Log((Px[i - 1] * Py[j - 1]) + eps) / Math.Log(2.0));
                    }
                }
                double res_a = Math.Sqrt(1.0 - (Math.Exp(-2.0 * (HXY2 - HXY))));
                if (!Double.IsNaN(res_a))
                {
                    res_set[itr] = res_a;
                    itr++;
                }
            }
            return Statistics.Mean(res_set);
        }
        public double GetMCC()
        {
            List<int> angles = glcm.Keys.ToList();
            angles.Sort();
            double[] res_set = new double[angles.Count];
            int itr = 0;
            foreach (int a in angles)
            {
                if (glcm[a] == null)
                {
                    continue;
                }
                double[][] glcm_a = glcm[a];
                double[] Px = (double[])coeffs[a]["Px"];
                double[] Py = (double[])coeffs[a]["Py"];
                Matrix<double> glcm_matrix= Matrix<double>.Build.Dense(nBins, nBins, Utils.Convert2DToArray(glcm_a));
                Matrix<double> px_matrix = Matrix<double>.Build.Dense(nBins, 1, Px);
                for (int i = 1; i < nBins; i++) 
                {
                    px_matrix = px_matrix.InsertColumn(px_matrix.ColumnCount, px_matrix.Column(0));
                }
                Matrix<double> Q_matrix = (glcm_matrix.SubMatrix(0,nBins,0,1)*(glcm_matrix.SubMatrix(0, 1, 0, nBins))).PointwiseDivide(px_matrix * Py[0] + eps);
                for (int i = 1; i < nBins; i++)
                {
                    Q_matrix += (glcm_matrix.SubMatrix(0, nBins, i, 1) * (glcm_matrix.SubMatrix(i, 1, 0, nBins))).PointwiseDivide(px_matrix * Py[i] + eps);
                }
                var evd = Q_matrix.Evd();
                MathNet.Numerics.LinearAlgebra.Vector<double> eigenValues = evd.EigenValues.Map(c => c.Real);
                if (eigenValues.Count < 2) 
                {
                    return 1;
                }
                double[] ei = eigenValues.ToArray();
                Array.Sort(ei);
                res_set[itr++] = Math.Sqrt(ei[eigenValues.Count - 2]);
            }
            return Statistics.Mean(res_set);
        }
        public double Calculate(GLCMFeatureType glcmType)
        {
            switch (glcmType)
            {
                case GLCMFeatureType.MaximumProbability:
                    return GetMaximumProbability();
                case GLCMFeatureType.JointAverage:
                    return GetJointAverage();
                case GLCMFeatureType.SumSquares:
                    return GetSumSquares();
                case GLCMFeatureType.JointEntropy:
                    return GetJointEntropy();
                case GLCMFeatureType.DifferenceAverage:
                    return GetDifferenceAverage();
                case GLCMFeatureType.DifferenceVariance:
                    return GetDifferenceVariance();
                case GLCMFeatureType.DifferenceEntropy:
                    return GetDifferenceEntropy();
                case GLCMFeatureType.SumAverage:
                    return GetSumAverage();
                case GLCMFeatureType.SumVariance:
                    return GetSumVariance();
                case GLCMFeatureType.SumEntropy:
                    return GetSumEntropy();
                case GLCMFeatureType.JointEnergy:
                    return GetJointEnergy();
                case GLCMFeatureType.Contrast:
                    return GetContrast();
                case GLCMFeatureType.InverseDifference:
                    return GetInverseDifference();
                case GLCMFeatureType.NormalizedInverseDifference:
                    return GetNormalisedInverseDifference();
                case GLCMFeatureType.InverseDifferenceMoment:
                    return GetInverseDifferenceMoment();
                case GLCMFeatureType.NormalizedInverseDifferenceMoment:
                    return GetNormalisedInverseDifferenceMoment();
                case GLCMFeatureType.InverseVariance:
                    return GetInverseVariance();
                case GLCMFeatureType.Correlation:
                    return GetCorrelation();
                case GLCMFeatureType.Autocorrection:
                    return GetAutocorrection();
                case GLCMFeatureType.ClusterTendency:
                    return GetClusterTendency();
                case GLCMFeatureType.ClusterShade:
                    return GetClusterShade();
                case GLCMFeatureType.ClusterProminence:
                    return GetClusterProminence();
                case GLCMFeatureType.InformationalMeasureOfCorrelation1:
                    return GetInformationalMeasureOfCorrelation1();
                case GLCMFeatureType.InformationalMeasureOfCorrelation2:
                    return GetInformationalMeasureOfCorrelation2();
                case GLCMFeatureType.MCC:
                    return GetMCC();
                default:
                    return double.NaN;
            }
        }
    }
}
