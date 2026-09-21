using System;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class MovementDensityMap
    {
        private readonly float[] _values;
        private readonly FlattenedHexCoordsConverter _coordsConverter;
    
        public MovementDensityMap(in FlattenedHexCoordsConverter coordsConverter)
        {
            _coordsConverter = coordsConverter;
            _values = new float[_coordsConverter.TotalTrianglesCount];
        }

        public float GetDensityUnsafe(IntTriangularPos tripos) => _values[_coordsConverter.TriangularToIndex(tripos)];

        public bool TryGetDensity(IntTriangularPos tripos, out float density)
        {
            if (!_coordsConverter.TryGetIndex(tripos, out var index))
            {
                density = float.MaxValue;
                return false;
            }

            density = _values[index];
            return true;
        }
    }
}
