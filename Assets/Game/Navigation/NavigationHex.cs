using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public class NavigationHex : IDisposable
    {
        public float2 Center => _center;
        public float3 CenterF3 => new(_center.x, 0f, _center.y);
        private readonly float2 _center;
        private readonly HashSet<IntTriangularPos> _passableTriangles = new();

        public NavigationHex(float2 center)
        {
            _center = center;
        }

        public void AddTriangle(IntTriangularPos pos) => _passableTriangles.Add(pos);
        public void RemoveTriangle(IntTriangularPos pos) => _passableTriangles.Remove(pos);
        public bool IsTrianglePassable(IntTriangularPos pos) => _passableTriangles.Contains(pos);

        public void Dispose()
        {
            _passableTriangles.Clear();
        }
    
    }
}
