using UnityEngine;

namespace CabinHome
{
    // Planning P=(east,north,elevation) becomes Unity U=(east,elevation-99,north).
    public static class SitePlan
    {
        public const float Width = 160f, Depth = 120f, Floor = 2f;
        public static float Height(float east, float north)
        {
            if (north < 38f) return .03f * north;
            if (north <= 86f) return 1.14f + .0125f * (north - 38f);
            float t = north - 86f;
            return 1.74f + .32f * t + (.018f + .004f * Mathf.Sin(east / 25f)) * t * t;
        }
        public static Vector3 Ground(float x, float z, float offset = 0f) => new(x, Height(x, z) + offset, z);
        public static readonly Vector3[] ReviewPositions = {
            new(100,9,26), new(122,52,10), new(80,3.7f,46), new(73.8f,3.6f,60)
        };
        public static readonly Vector3[] ReviewTargets = {
            new(77,4,62), new(80,2,62), new(76,3.5f,60), new(82,6,10)
        };
    }
}
