using System;
using System.Collections.Generic;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public class NavigationHex : IDisposable
    {
        public readonly float2 Center;
        private readonly HashSet<NavTrianglePos> _passableTriangles = new();

        public NavigationHex(float2 center)
        {
            Center = center;
        }

        public void AddTriangle(NavTrianglePos pos) => _passableTriangles.Add(pos);
        public void RemoveTriangle(NavTrianglePos pos) => _passableTriangles.Remove(pos);
        public bool IsTrianglePassable(NavTrianglePos pos) => _passableTriangles.Contains(pos);

        public void Dispose()
        {
            _passableTriangles.Clear();
        }
    
    }
}
