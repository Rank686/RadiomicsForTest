using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiomicsNet.Features
{
    public enum GLDZMFeatureType
    {
        All,
        SmallDistanceEmphasis,
        LargeDistanceEmphasis,
        LowGrayLevelZoneEmphasis,
        HighGrayLevelZoneEmphasis,
        SmallDistanceLowGrayLevelEmphasis,
        SmallDistanceHighGrayLevelEmphasis,
        LargeDistanceLowGrayLevelEmphasis,
        LargeDistanceHighGrayLevelEmphasis,
        GrayLevelNonUniformity,
        GrayLevelNonUniformityNormalized,
        ZoneDistanceNonUniformity,
        ZoneDistanceNonUniformityNormalized,
        ZonePercentage,
        GrayLevelVariance,
        ZoneDistanceVariance,
        ZoneDistanceEntropy,
    }
}
