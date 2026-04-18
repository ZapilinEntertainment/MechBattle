using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class SubdivideTriangleCommand
    {
        private const float ORTHOCENTER_CF = 2f / 3f; 

        public readonly struct SmallTriangleData
        {
            public readonly float2 WorldPos;
            public readonly bool IsPeak;
            public float3 WorldPosV3 => new float3(WorldPos.x, 0f, WorldPos.y);

            public SmallTriangleData(float2 pos, bool isPeak)
            {
                WorldPos = pos;
                IsPeak = isPeak;
            }
        }

        // why protocol and no collection creation: multiple re-using in scenarios
        public struct TriangleSubdivisionProtocol
        {
            public float TriangleHeight;
            public int RaycastTrianglesPerEdge;
            public NativeArray<SmallTriangleData> Centers;
        }

        public static NativeArray<SmallTriangleData> CreateDataArray(int trianglesPerEdge, Allocator allocator) => 
            new NativeArray<SmallTriangleData>(trianglesPerEdge * trianglesPerEdge, allocator, NativeArrayOptions.UninitializedMemory);


        // TODO: Rework to use TriangleSubdivisionEnumerator to have correct order!

        [BurstCompile]
        public static void Execute(
             float2 center,
             bool isPeakTriangle,
             TriangleSubdivisionProtocol protocol)
        {
            // divide triangle into n^2 smaller congruent triangles
            var subdivisionsCount = protocol.RaycastTrianglesPerEdge;
            var centers = protocol.Centers;

            if (subdivisionsCount == 0 || subdivisionsCount == 1)
            {
                centers[0] = new(center, isPeakTriangle);
                return;
            }

            var bigTriangleHeight = protocol.TriangleHeight;
            var smallTriangleHeight = bigTriangleHeight / subdivisionsCount;
            var smallTriangleEdgeLength = (2f * smallTriangleHeight) * NavigationConstants.DIV_SQRT_OF_THREE;


            var zeroPos = center;
            zeroPos.y += (bigTriangleHeight - smallTriangleHeight) * ORTHOCENTER_CF * (isPeakTriangle ? 1f : -1f);
            centers[0] = new(zeroPos, isPeakTriangle);

            var nextCenterDir = math.mul(
                quaternion.AxisAngle(math.down(), math.radians(isPeakTriangle ? 150f : 30f)),
                new float3(0, 0, smallTriangleEdgeLength))
                .xz;

            var index = 1;

            var offset = (isPeakTriangle ? 1 : -1) * smallTriangleHeight * ORTHOCENTER_CF * 0.5f;

            for (var row = 2; row <= subdivisionsCount; row++)
            {
                var startPos = zeroPos + nextCenterDir * (row - 1);
                var trianglesInRow = 2 * row - 1;

                for (var i = 0; i < trianglesInRow; i++)
                {
                    bool isCurrentPeak;
                    float yOffset;
                    if (isPeakTriangle)
                    {
                        isCurrentPeak = i % 2 == 0;
                        yOffset = ((i+1) % 2) * offset;
                    }
                    else
                    {
                        isCurrentPeak = i % 2 != 0;
                        yOffset = (i % 2) * offset;
                    }

                    // magic calculation
                    if (isPeakTriangle) 
                        yOffset += (isCurrentPeak ? - 1f : 1f) *  NavigationConstants.DIV_THREE * smallTriangleHeight;

                    centers[index++] = new(
                        new(startPos.x + i * smallTriangleEdgeLength * 0.5f,
                         startPos.y + yOffset),
                         isCurrentPeak);
                }
            }
        }
    }
}
