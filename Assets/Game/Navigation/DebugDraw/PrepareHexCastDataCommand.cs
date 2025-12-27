using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    // showing cast points and returns locked triangles (standartized coords)
    internal static class PrepareHexCastDataCommand
    {
        internal struct HexRaycastDrawProtocol
        {
            public int TrianglesInHexCount;
            public int TrianglesPerHexEdge;
            public int RaycastSubdivisionsPerEdge;
            public QueryParameters CastQueryParameters;
            public float TriangleEdgeSize;
            public float IntersectionPercentForLock;

            public List<SphereDrawData> DrawData;
        }

        internal static HashSet<IntTriangularPos> Execute(float2 hexCenter, in HexRaycastDrawProtocol protocol)
        {
            // do obstacles cast
            var raycastResolution = protocol.RaycastSubdivisionsPerEdge;
            var raycastCommandsCount = protocol.TrianglesInHexCount * raycastResolution * raycastResolution;
            var trianglePositions = new NativeArray<IntTriangularPos>(protocol.TrianglesInHexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var raycastCommands = new NativeList<RaycastCommand>(raycastCommandsCount, Allocator.TempJob);
            var raycastsCount = raycastResolution * raycastResolution;
            var raycastPositions = new NativeArray<float2>(raycastsCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NavigationCaster.PrepareRaycastCommands(hexCenter, new()
            {
                CastingHeight = 100f,
                CastingRayLength = 200f,
                RaycastCommands = raycastCommands,
                TempPositionsArray = trianglePositions,
                QueryParameters = protocol.CastQueryParameters,
                TriangleEdgeSize = protocol.TriangleEdgeSize,
                HexTrianglesPerEdge = protocol.TrianglesPerHexEdge,
                RaycastTrianglesPerEdge = raycastResolution,
                TempRaycastPointsArray = raycastPositions
            });
            trianglePositions.Dispose();
            raycastPositions.Dispose();

            var raycastResults = new NativeArray<RaycastHit>(raycastCommandsCount, Allocator.TempJob);
            var castJobHandle = RaycastCommand.ScheduleBatch(raycastCommands.AsArray(), raycastResults, 16);

            // draw obstacles
            castJobHandle.Complete();
            raycastCommands.Dispose();
            var intersectionsCount = new Dictionary<IntTriangularPos, int>();

            var drawData = protocol.DrawData;
            var writeGizmosData = drawData != null;

            for (var i = 0; i < raycastCommandsCount; i++)
            {
                var result = raycastResults[i];
                if (result.collider == null)
                    continue;
                if (!result.collider.gameObject.isStatic)
                {
                    var trianglePos = TriangularMath.WorldToTrianglePos(result.point, protocol.TriangleEdgeSize);
                    intersectionsCount.TryGetValue(trianglePos, out var value);
                    intersectionsCount[trianglePos] = value + 1;

                    if (writeGizmosData)
                        protocol.DrawData.Add(new(result.point, DebugColor.Red, 0.5f));
                }
                else
                {
                    if (writeGizmosData)
                        protocol.DrawData.Add(new(result.point, DebugColor.Green, 0.5f));
                }
            }

            raycastResults.Dispose();

            var lockedTriangles = new HashSet<IntTriangularPos>();
            foreach (var triKvp in intersectionsCount)
            {
                if (triKvp.Value / (float)raycastsCount >= protocol.IntersectionPercentForLock)
                    lockedTriangles.Add(triKvp.Key.ToStandartized());
            }
            return lockedTriangles;
        }

    }
}
