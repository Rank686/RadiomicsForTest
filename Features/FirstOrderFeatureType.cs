using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Features
{
    public enum FirstOrderFeatureType
    {
        Mean,
        Variance,
        Skewness,
        Kurtosis,//excess kurtosis
        Median,
        Minimum,
        Percentile10,
        Percentile90,
        Maximum,
        Peak,
        Interquartile,
        Range,
        MeanAbsoluteDeviation,
        RobustMeanAbsoluteDeviation,
        Energy,
        RootMeanSquared,
        TotalEnergy,//origin
        StandardDeviation,//origin
        Entropy,
        Uniformity
    }
}
