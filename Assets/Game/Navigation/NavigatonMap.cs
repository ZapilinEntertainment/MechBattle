using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigatonMap : IDisposable
    {
        public readonly float3 Center;
        public readonly int HexWidth;
        public readonly int HexLength;
        public readonly float HexEdgeSize;
        public readonly float HexInnerRadius;
        public readonly float3 OrthX = math.normalize(math.mul(quaternion.AxisAngle(math.up(), math.radians(120f)), math.forward()));
        public readonly float3 OrthY = math.forward();
        public IReadOnlyDictionary<int2, NavigationHex> Hexes => _hexes;

        private readonly Dictionary<int2, NavigationHex> _hexes = new();
    
        public NavigatonMap(float3 center, int hexWidth, int hexLength, float hexEdgeSize)
        {
            Center = center;
            HexWidth = hexWidth;
            HexLength = hexLength;

            HexEdgeSize = hexEdgeSize;
            HexInnerRadius = hexEdgeSize * math.sqrt(3f) * 0.5f;
        }

        public void AddHex(int2 pos)
        {
            var center = HexPosToWorld(pos);
            _hexes[pos] = new(center.xz);
        }
        public void RemoveHex(int2 pos) => _hexes.Remove(pos);

        public void Dispose()
        {
            _hexes.Clear();
        }

        public float3 HexPosToWorld(int2 hexPos) => Center + hexPos.x * 2f * HexInnerRadius * OrthX  + hexPos.y * 2f * HexInnerRadius * OrthY;
    }
}
