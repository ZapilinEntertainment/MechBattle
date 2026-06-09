using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct EdgePositionsDefineJob : IJobParallelFor
    {
        public int TrianglesPerNode;
        public int HexRadius;
        public float HexEdgeSize;
        [ReadOnly] public NativeArray<HexEdgeKey> CalculatingNodes;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<int4> Results;

        public void Execute(int index)
        {
            var writeIndex = index * TrianglesPerNode;

            var node = CalculatingNodes[index];
            var hexPos = new NavigationHexPosition(node, HexEdgeSize, HexRadius);
            AddEdgePositions(node, hexPos, writeIndex);

            // opposite cells not needed - they will be taken by direction        
        }

        private void AddEdgePositions(HexEdgeKey node, NavigationHexPosition hexPos, int startIndex)
        {
            var peakDir = (int)node.Edge.ToNeighbourDirectionFromPeak();
            var valleyDir = (int)node.Edge.ToNeighbourDirectionFromValley();

            switch (node.Edge)
            {
                case HexEdge.TopRight: UpdateMask<TopRightEdgeEnumerationLogic>(new(HexRadius, hexPos),startIndex, peakDir, valleyDir); break;
                case HexEdge.BottomRight: UpdateMask<BottomRightEdgeEnumerationLogic>(new(HexRadius, hexPos), startIndex, peakDir, valleyDir); break;
                case HexEdge.Bottom: UpdateMask<BottomEdgeEnumerationLogic>(new(HexRadius, hexPos), startIndex, peakDir, valleyDir); break;
                case HexEdge.BottomLeft: UpdateMask<BottomLeftEdgeEnumerationLogic>(new(HexRadius, hexPos), startIndex, peakDir, valleyDir); break;
                case HexEdge.TopLeft: UpdateMask<TopLeftEdgeEnumerationLogic>(new(HexRadius, hexPos), startIndex, peakDir, valleyDir); break;
                default: UpdateMask<TopEdgeEnumerationLogic>(new(HexRadius, hexPos), startIndex, peakDir, valleyDir); break;
            }
        }

        private void UpdateMask<T>(EdgeEnumerator<T> enumerator,int startIndex, int peakDir, int valleyDir) where T : unmanaged, IEdgeEnumerationLogic
        {
            var i = 0;
            foreach (var tripos in enumerator)
            {
                Results[startIndex + i] = new(tripos, tripos.IsPeak ? peakDir : valleyDir);
                i++;
            }
        }
    }
}
