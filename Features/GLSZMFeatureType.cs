using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiomicsNet.Features
{
    public enum GLSZMFeatureType
    {
        All,
        SmallZoneEmphasis,
        LargeZoneEmphasis,
        LowGrayLevelZoneEmphasis,
        HighGrayLevelZoneEmphasis,
        SmallZoneLowGrayLevelEmphasis,
        SmallZoneHighGrayLevelEmphasis,
        LargeZoneLowGrayLevelEmphasis,
        LargeZoneHighGrayLevelEmphasis,
        GrayLevelNonUniformity,
        GrayLevelNonUniformityNormalized,
        SizeZoneNonUniformity,
        SizeZoneNonUniformityNormalized,
        ZonePercentage,
        GrayLevelVariance,
        ZoneSizeVariance,
        ZoneSizeEntropy,
    }
}
