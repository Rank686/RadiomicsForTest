using MathNet.Numerics.LinearAlgebra;
using Radiomics.Net.Features;
using Radiomics.Net.ImageProcess;
using System.Reflection;

namespace Radiomics.Net.Tests;

internal static class GlcmExpectedCalculator
{
    public static IReadOnlyDictionary<GLCMFeatureType, double> Compute(GLCMFeatures features)
    {
        var glcmField = typeof(GLCMFeatures).GetField("glcm", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to access GLCM matrices.");
        var glcm = (Dictionary<int, double[][]>)glcmField.GetValue(features)!;
        var nBinsField = typeof(GLCMFeatures).GetField("nBins", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to access nBins.");
        var nBins = (int)nBinsField.GetValue(features)!;
        var eps = Utils.GetDoubleUlp(1.0);

        var matrices = glcm.OrderBy(kvp => kvp.Key)
            .Select(kvp => new KeyValuePair<int, GlcmDerivedData?>(kvp.Key, kvp.Value == null ? null : ComputeDerivedData(kvp.Value, nBins)))
            .ToList();

        return Enum.GetValues<GLCMFeatureType>().ToDictionary(
            feature => feature,
            feature => feature switch
            {
                GLCMFeatureType.MaximumProbability => Average(matrices, data => GetMaximum(data.Matrix)),
                GLCMFeatureType.JointAverage => Average(matrices, data => GetJointAverage(data.Matrix)),
                GLCMFeatureType.SumSquares => Average(matrices, data => GetSumSquares(data.Matrix)),
                GLCMFeatureType.JointEntropy => Average(matrices, data => GetJointEntropy(data.Matrix, eps)),
                GLCMFeatureType.JointEnergy => Average(matrices, data => GetJointEnergy(data.Matrix)),
                GLCMFeatureType.DifferenceAverage => Average(matrices, data => GetDifferenceAverage(data.PxSubY)),
                GLCMFeatureType.DifferenceVariance => Average(matrices, data => GetDifferenceVariance(data.PxSubY)),
                GLCMFeatureType.DifferenceEntropy => Average(matrices, data => GetDifferenceEntropy(data.PxSubY, eps)),
                GLCMFeatureType.SumAverage => Average(matrices, data => GetSumAverage(data.PxAddY)),
                GLCMFeatureType.SumVariance => Average(matrices, data => GetSumVariance(data.PxAddY)),
                GLCMFeatureType.SumEntropy => Average(matrices, data => GetSumEntropy(data.PxAddY, eps)),
                GLCMFeatureType.Contrast => Average(matrices, data => GetContrast(data.Matrix)),
                GLCMFeatureType.InverseDifference => Average(matrices, data => GetInverseDifference(data.PxSubY)),
                GLCMFeatureType.NormalizedInverseDifference => Average(matrices, data => GetNormalisedInverseDifference(data.PxSubY, nBins)),
                GLCMFeatureType.InverseDifferenceMoment => Average(matrices, data => GetInverseDifferenceMoment(data.Matrix)),
                GLCMFeatureType.NormalizedInverseDifferenceMoment => Average(matrices, data => GetNormalisedInverseDifferenceMoment(data.Matrix, nBins)),
                GLCMFeatureType.InverseVariance => Average(matrices, data => GetInverseVariance(data.PxSubY)),
                GLCMFeatureType.Correlation => Average(matrices, data => GetCorrelation(data)),
                GLCMFeatureType.Autocorrection => Average(matrices, data => GetAutocorrelation(data.Matrix)),
                GLCMFeatureType.ClusterTendency => Average(matrices, data => GetClusterMoment(data, 2)),
                GLCMFeatureType.ClusterShade => Average(matrices, data => GetClusterMoment(data, 3)),
                GLCMFeatureType.ClusterProminence => Average(matrices, data => GetClusterMoment(data, 4)),
                GLCMFeatureType.InformationalMeasureOfCorrelation1 => Average(matrices, data => GetImc1(data, eps)),
                GLCMFeatureType.InformationalMeasureOfCorrelation2 => Average(matrices, data => GetImc2(data, eps)),
                GLCMFeatureType.MCC => Average(matrices, data => GetMcc(data)),
                _ => 0d
            });
    }

    private static double Average(IReadOnlyList<KeyValuePair<int, GlcmDerivedData?>> matrices, Func<GlcmDerivedData, double> selector)
    {
        if (matrices.Count == 0)
        {
            return 0d;
        }

        double sum = 0;
        foreach (var entry in matrices)
        {
            if (entry.Value is { } data)
            {
                sum += selector(data);
            }
        }
        return sum / matrices.Count;
    }

    private static GlcmDerivedData ComputeDerivedData(double[][] matrix, int nBins)
    {
        var px = new double[nBins];
        var py = new double[nBins];
        for (int i = 0; i < nBins; i++)
        {
            for (int j = 0; j < nBins; j++)
            {
                var value = matrix[i][j];
                px[i] += value;
                py[j] += value;
            }
        }

        double meanX = 0;
        double meanY = 0;
        for (int i = 0; i < nBins; i++)
        {
            meanX += (i + 1) * px[i];
            meanY += (i + 1) * py[i];
        }

        double stdDevX = 0;
        double stdDevY = 0;
        for (int i = 0; i < nBins; i++)
        {
            stdDevX += Math.Pow((i + 1) - meanX, 2) * px[i];
            stdDevY += Math.Pow((i + 1) - meanY, 2) * py[i];
        }
        stdDevX = Math.Sqrt(stdDevX);
        stdDevY = Math.Sqrt(stdDevY);

        var pxAddY = new double[(nBins * 2) - 1];
        for (int k = 2; k <= nBins * 2; k++)
        {
            for (int i = 1; i <= nBins; i++)
            {
                for (int j = 1; j <= nBins; j++)
                {
                    if (k == i + j)
                    {
                        pxAddY[k - 2] += matrix[i - 1][j - 1];
                    }
                }
            }
        }

        var pxSubY = new double[nBins];
        for (int k = 0; k < nBins; k++)
        {
            for (int i = 1; i <= nBins; i++)
            {
                for (int j = 1; j <= nBins; j++)
                {
                    if (k == Math.Abs(i - j))
                    {
                        pxSubY[k] += matrix[i - 1][j - 1];
                    }
                }
            }
        }

        return new GlcmDerivedData(matrix, px, py, pxAddY, pxSubY, meanX, meanY, stdDevX, stdDevY);
    }

    private static double GetMaximum(double[][] matrix)
    {
        double max = 0;
        foreach (var row in matrix)
        {
            foreach (var value in row)
            {
                if (value > max)
                {
                    max = value;
                }
            }
        }
        return max;
    }

    private static double GetJointAverage(double[][] matrix)
    {
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += matrix[i][j] * (i + 1);
            }
        }
        return result;
    }

    private static double GetSumSquares(double[][] matrix)
    {
        var mean = GetJointAverage(matrix);
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += Math.Pow((i + 1) - mean, 2) * matrix[i][j];
            }
        }
        return result;
    }

    private static double GetJointEntropy(double[][] matrix, double eps)
    {
        double entropy = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                var value = matrix[i][j];
                if (value > 0)
                {
                    entropy -= value * (Math.Log(value + eps) / Math.Log(2d));
                }
            }
        }
        return entropy;
    }

    private static double GetJointEnergy(double[][] matrix)
    {
        double energy = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                var value = matrix[i][j];
                energy += value * value;
            }
        }
        return energy;
    }

    private static double GetDifferenceAverage(double[] pxSubY)
    {
        double result = 0;
        for (int k = 0; k < pxSubY.Length; k++)
        {
            result += pxSubY[k] * k;
        }
        return result;
    }

    private static double GetDifferenceVariance(double[] pxSubY)
    {
        var mean = GetDifferenceAverage(pxSubY);
        double result = 0;
        for (int k = 0; k < pxSubY.Length; k++)
        {
            result += Math.Pow(k - mean, 2) * pxSubY[k];
        }
        return result;
    }

    private static double GetDifferenceEntropy(double[] pxSubY, double eps)
    {
        double result = 0;
        for (int k = 0; k < pxSubY.Length; k++)
        {
            var value = pxSubY[k];
            if (value > 0)
            {
                result -= value * (Math.Log(value + eps) / Math.Log(2d));
            }
        }
        return result;
    }

    private static double GetSumAverage(double[] pxAddY)
    {
        double result = 0;
        for (int k = 0; k < pxAddY.Length; k++)
        {
            result += pxAddY[k] * (k + 2);
        }
        return result;
    }

    private static double GetSumVariance(double[] pxAddY)
    {
        var mean = GetSumAverage(pxAddY);
        double result = 0;
        for (int k = 0; k < pxAddY.Length; k++)
        {
            result += Math.Pow((k + 2) - mean, 2) * pxAddY[k];
        }
        return result;
    }

    private static double GetSumEntropy(double[] pxAddY, double eps)
    {
        double result = 0;
        for (int k = 0; k < pxAddY.Length; k++)
        {
            var value = pxAddY[k];
            if (value > 0)
            {
                result -= value * (Math.Log(value + eps) / Math.Log(2d));
            }
        }
        return result;
    }

    private static double GetContrast(double[][] matrix)
    {
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += Math.Pow(i - j, 2) * matrix[i][j];
            }
        }
        return result;
    }

    private static double GetInverseDifference(double[] pxSubY)
    {
        double result = 0;
        for (int k = 0; k < pxSubY.Length; k++)
        {
            result += pxSubY[k] / (1d + k);
        }
        return result;
    }

    private static double GetNormalisedInverseDifference(double[] pxSubY, int nBins)
    {
        double result = 0;
        for (int k = 0; k < pxSubY.Length; k++)
        {
            result += pxSubY[k] / (1d + k / (double)nBins);
        }
        return result;
    }

    private static double GetInverseDifferenceMoment(double[][] matrix)
    {
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += matrix[i][j] / (1d + Math.Pow(i - j, 2));
            }
        }
        return result;
    }

    private static double GetNormalisedInverseDifferenceMoment(double[][] matrix, int nBins)
    {
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += matrix[i][j] / (1d + (Math.Pow(i - j, 2) / Math.Pow(nBins, 2)));
            }
        }
        return result;
    }

    private static double GetInverseVariance(double[] pxSubY)
    {
        double result = 0;
        for (int k = 1; k < pxSubY.Length; k++)
        {
            if (k == 0)
            {
                continue;
            }
            result += pxSubY[k] / (k * k);
        }
        return result;
    }

    private static double GetCorrelation(GlcmDerivedData data)
    {
        var matrix = data.Matrix;
        double numerator = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                numerator += ((i + 1) - data.MeanX) * ((j + 1) - data.MeanY) * matrix[i][j];
            }
        }
        if (data.StdDevX == 0 || data.StdDevY == 0)
        {
            return double.NaN;
        }
        return (1d / (data.StdDevX * data.StdDevY)) * numerator;
    }

    private static double GetAutocorrelation(double[][] matrix)
    {
        double result = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += matrix[i][j] * (i + 1) * (j + 1);
            }
        }
        return result;
    }

    private static double GetClusterMoment(GlcmDerivedData data, int order)
    {
        double result = 0;
        var matrix = data.Matrix;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                result += Math.Pow((i + 1) + (j + 1) - data.MeanX - data.MeanY, order) * matrix[i][j];
            }
        }
        return result;
    }

    private static double GetImc1(GlcmDerivedData data, double eps)
    {
        double hx = 0;
        double hxy = 0;
        double hxy1 = 0;
        var matrix = data.Matrix;
        for (int i = 0; i < matrix.Length; i++)
        {
            if (data.Px[i] > 0)
            {
                hx -= data.Px[i] * (Math.Log(data.Px[i] + eps) / Math.Log(2d));
            }
            for (int j = 0; j < matrix[i].Length; j++)
            {
                var value = matrix[i][j];
                if (value > 0)
                {
                    hxy -= value * (Math.Log(value + eps) / Math.Log(2d));
                }
                hxy1 -= value * (Math.Log((data.Px[i] * data.Py[j]) + eps) / Math.Log(2d));
            }
        }
        return hx == 0 ? double.NaN : (hxy - hxy1) / hx;
    }

    private static double GetImc2(GlcmDerivedData data, double eps)
    {
        double hxy = 0;
        double hxy2 = 0;
        var matrix = data.Matrix;
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                var value = matrix[i][j];
                if (value > 0)
                {
                    hxy -= value * (Math.Log(value + eps) / Math.Log(2d));
                }
                var product = data.Px[i] * data.Py[j];
                hxy2 -= product * (Math.Log(product + eps) / Math.Log(2d));
            }
        }
        var exponent = -2d * (hxy2 - hxy);
        var inner = 1d - Math.Exp(exponent);
        return double.IsNaN(inner) || inner < 0 ? double.NaN : Math.Sqrt(inner);
    }

    private static double GetMcc(GlcmDerivedData data)
    {
        var matrix = data.Matrix;
        var nBins = matrix.Length;
        var glcmMatrix = Matrix<double>.Build.Dense(nBins, nBins, Utils.Convert2DToArray(matrix));
        var pxMatrix = Matrix<double>.Build.Dense(nBins, 1, data.Px);
        for (int i = 1; i < nBins; i++)
        {
            pxMatrix = pxMatrix.InsertColumn(pxMatrix.ColumnCount, pxMatrix.Column(0));
        }

        var py = data.Py;
        var qMatrix = (glcmMatrix.SubMatrix(0, nBins, 0, 1) * glcmMatrix.SubMatrix(0, 1, 0, nBins))
            .PointwiseDivide(pxMatrix * py[0] + Utils.GetDoubleUlp(1.0));
        for (int i = 1; i < nBins; i++)
        {
            qMatrix += (glcmMatrix.SubMatrix(0, nBins, i, 1) * glcmMatrix.SubMatrix(i, 1, 0, nBins))
                .PointwiseDivide(pxMatrix * py[i] + Utils.GetDoubleUlp(1.0));
        }

        var eigenValues = qMatrix.Evd().EigenValues.Map(c => c.Real).ToArray();
        Array.Sort(eigenValues);
        if (eigenValues.Length < 2)
        {
            return 1d;
        }
        return Math.Sqrt(Math.Max(0d, eigenValues[^2]));
    }

    private sealed class GlcmDerivedData
    {
        public GlcmDerivedData(double[][] matrix, double[] px, double[] py, double[] pxAddY, double[] pxSubY, double meanX, double meanY, double stdDevX, double stdDevY)
        {
            Matrix = matrix;
            Px = px;
            Py = py;
            PxAddY = pxAddY;
            PxSubY = pxSubY;
            MeanX = meanX;
            MeanY = meanY;
            StdDevX = stdDevX;
            StdDevY = stdDevY;
        }

        public double[][] Matrix { get; }
        public double[] Px { get; }
        public double[] Py { get; }
        public double[] PxAddY { get; }
        public double[] PxSubY { get; }
        public double MeanX { get; }
        public double MeanY { get; }
        public double StdDevX { get; }
        public double StdDevY { get; }
    }
}
