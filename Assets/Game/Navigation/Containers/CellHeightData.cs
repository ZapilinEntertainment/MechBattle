using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CellHeightData
    {
        public readonly float3 VertexA;
        public readonly float3 VertexB;
        public readonly float3 VertexC;
        public readonly float3 VertexM;
        public readonly bool IsFlat;
        private readonly bool3 _excessData;

        public CellHeightData(float3 vertexA, float3 vertexB, float3 vertexC)
        {
            VertexA = vertexA;
            VertexB = vertexB;
            VertexC = vertexC;
            IsFlat = true;
            VertexM = default;
            _excessData = false;
        }

        public CellHeightData(float3 vertexA, float3 vertexB, float3 vertexC, float3 vertexM)
        {
            VertexA = vertexA;
            VertexB = vertexB;
            VertexC = vertexC;
            VertexM = vertexM;

            IsFlat = false;
            _excessData = false;
        }

    }
}
