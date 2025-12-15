using System.Collections.Generic;   
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum TriangularDirection
    {
        Up,
        UpRight,
        DownRight,
        Down,
        DownLeft,
        UpLeft,    
    }

    public static class TriangularDirectionExtension
    {
        public static IReadOnlyDictionary<TriangularDirection, int3> PeakVectors => _peakVectors;
        public static IReadOnlyDictionary<TriangularDirection, int3> ValleyVectors => _valleyVectors;
        private static readonly Dictionary<TriangularDirection, int3> _peakVectors = new()
        {
            {TriangularDirection.Up, new (0, 2, 0)  },
            {TriangularDirection.UpRight, new (0,  1,  1) },
            {TriangularDirection.Down, new (1, 0, 1) },
            {TriangularDirection.DownRight,  new (0, 0, 2) },
            {TriangularDirection.DownLeft, new ( 2, 0, 0)  },
            {TriangularDirection.UpLeft, new (1, 1, 0) }
        };
        private static readonly Dictionary<TriangularDirection, int3> _valleyVectors = new()
        {
            {TriangularDirection.Up,  new (-1, 0,  -1) },
            {TriangularDirection.UpRight, new ( - 2, 0, 0)},
            {TriangularDirection.Down, new (0,  - 2, 0) },
            {TriangularDirection.DownRight,  new ( - 1,  - 1, 0) },
            {TriangularDirection.DownLeft, new (0,  - 1,  - 1) },
            {TriangularDirection.UpLeft, new (0, 0,  - 2) }
        };

        public static int3 ToEdgeNeighbourVector(this TriangularDirection direction) => _peakVectors[direction];
    }
}
