using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Features
{
    public enum GLCMFeatureType
    {
        MaximumProbability,
        JointAverage,
        SumSquares,//Sum of Squares
        JointEntropy,
        JointEnergy,
        DifferenceAverage,
        DifferenceVariance,
        DifferenceEntropy,
        SumAverage,
        SumVariance,
        SumEntropy,
        Contrast,
        InverseDifference,
        NormalizedInverseDifference,
        InverseDifferenceMoment,
        NormalizedInverseDifferenceMoment,
        InverseVariance,
        Correlation,
        Autocorrection,
        ClusterTendency,
        ClusterShade,
        ClusterProminence,
        InformationalMeasureOfCorrelation1,
        InformationalMeasureOfCorrelation2,
        MCC
    }
}
