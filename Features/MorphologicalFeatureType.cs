using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiomicsNet.Features
{
    public enum MorphologicalFeatureType
    {
        All,
        VolumeMesh,
        VolumeVoxelCounting,
        SurfaceAreaMesh,
        SurfaceToVolumeRatio,
        Compactness1,
        Compactness2,
        SphericalDisproportion,
        Sphericity,
        Asphericity,
        CentreOfMassShift,
        Maximum3DDiameter,
        MajorAxisLength,
        MinorAxisLength,
        LeastAxisLength,
        Elongation,
        Flatness,
        VolumeDensity_AxisAlignedBoundingBox,
        AreaDensity_AxisAlignedBoundingBox,
        VolumeDensity_OrientedMinimumBoundingBox,
        AreaDensity_OrientedMinimumBoundingBox,
        VolumeDensity_ApproximateEnclosingEllipsoid,
        AreaDensity_ApproximateEnclosingEllipsoid,
        VolumeDensity_MinimumVolumeEnclosingEllipsoid,
        AreaDensity_MinimumVolumeEnclosingEllipsoid,
        VolumeDensity_ConvexHull,
        AreaDensity_ConvexHull,
        IntegratedIntensity,
        MoransIIndex,
        GearysCMeasure

    }
}
