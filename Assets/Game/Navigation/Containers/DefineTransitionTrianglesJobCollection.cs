using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class DefineTransitionTrianglesJobCollection : IDisposable
    {
        public NativeArray<HexEdgeKey> CalculatingNodes;
        public NativeArray<int4> Results;
        public int TrianglesPerNode { get; private set; }
        private readonly Allocator _allocator;

        public DefineTransitionTrianglesJobCollection(Allocator allocator)
        {
            _allocator = allocator;
        }

        public void Update(IReadOnlyCollection<HexEdgeKey> nodes, int trianglesPerEdge)
        {
            var nodesLength = nodes.Count;
            if (!CalculatingNodes.IsCreated || CalculatingNodes.Length != nodes.Count)
            {
                CalculatingNodes.Dispose();
                CalculatingNodes = new(nodes.Count, _allocator, NativeArrayOptions.UninitializedMemory);

                var index = 0;
                foreach (var node in nodes)
                    CalculatingNodes[index++] = node;
            }

            TrianglesPerNode = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);
            var resultsCount = TrianglesPerNode * CalculatingNodes.Length;

            if (!Results.IsCreated || Results.Length != resultsCount)
            {
                Results.Dispose();
                Results = new(resultsCount, _allocator);
            }

        }

        public void Dispose()
        {
            CalculatingNodes.Dispose();
            Results.Dispose();
        }
    }
}
