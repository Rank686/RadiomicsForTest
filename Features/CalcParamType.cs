using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Features
{
    public enum CalcParamType
    {
        INT_label,
        INT_interpolation2D,    //NEAREST_NEIGHBOR=0, BILINEAR=1, BICUBIC=2;
        INT_interpolation_mask2D,//for original img
        INT_interpolation3D, //TRILINEAR=100, NEAREST3D=101, TRICUBIC_SPLINE=102, TRICUBIC_POLYNOMIAL=103
        INT_interpolation_mask3D,
        DOUBLE_Mask_PartialVolumeThareshold,//0.5
        BOOL_USE_FixedBinNumber,
        DOUBLE_binWidth,
        INT_binCount,
        BOOL_normalize,
        BOOL_removeOutliers,
        DOUBLE_zScore,
        DOUBLE_normalizeScale,
        DOUBLE_densityShift,
        DOUBLE_rangeMax,
        DOUBLE_rangeMin,
        INT_IVH_binCount,
        DOUBLE_IVH_binWidth,
        INT_IVH_MODE,
        INT_alpha,//NGLDM, NGTDM search range 
        INT_deltaGLCM,//GLCM distance
        INT_deltaNGTDM,//NGTDM distance
        INT_deltaNGLDM,//NGLDM distance
        STRING_weightingNorm,
        INTARRAY_box_sizes,//to calsulate fractal D
        DOUBLEARRAY_resamplingFactorXYZ,
        BOOL_force2D,//slice by slice calculation
        BOOL_activate_no_default_features,
        BOOL_enableIntensityBasedStatistics,
        BOOL_enableLocalIntensityFeatures,
        BOOL_enableIntensityHistogram,
        BOOL_enableIntensityVolumeHistogram,
        BOOL_enableMorphological,
        BOOL_enableShape2D,
        BOOL_enableGLCM,
        BOOL_enableGLRLM,
        BOOL_enableGLSZM,
        BOOL_enableGLDZM,
        BOOL_enableNGTDM,
        BOOL_enableNGLDM,
        BOOL_enableHomological,
        BOOL_enableFractal,
        BOOL_enableOperationalInfo,//date, os, version, modality name, manufacturer, 
        BOOL_enableDiagnostics,
        STRING_waveletFilterName,//小波变换名称：coif 1-5, biortho 1-15, daub 1-20,revbiortho 1-15, sym 2-20, haar1, meyer1
    }
}
