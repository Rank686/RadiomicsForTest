using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiomicsNet.Features
{
    public enum HistogramFeatureType
    {
        All,
        MeanDiscretisedIntensity,
        Variance,
        Skewness,
        Kurtosis,
        Median,
        Minimum,
        Percentile10,
        Percentile90,
        Maximum,
        Mode,//histogram
        Interquartile,
        Range,
        MeanAbsoluteDeviation,
        RobustMeanAbsoluteDeviation,
        MedianAbsoluteDeviation,
        CoefficientOfVariation,
        QuartileCoefficientOfDispersion,
        //histogram
        Entropy,
        Uniformity,
        MaximumHistogramGradient,
        MaximumHistogramGradientIntensity,
        MinimumHistogramGradient,
        MinimumHistogramGradientIntensity,
    }
}
