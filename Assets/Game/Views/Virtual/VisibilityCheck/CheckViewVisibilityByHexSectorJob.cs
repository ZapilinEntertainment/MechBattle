using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct CheckViewVisibilityByHexSectorJob : IJob
    {
        public NativeFilter Filter;
        public NativeStash<PositionComponent> Positions;
        public NativeStash<ViewVisibilityStateComponent> VisibilityStates;
        public NativeHashMap<int2, int> SectorVisibilityMasks;
        public float TriangleHeight;
        public float HexEdgeLength;
        public int HexRadius;
        private const int REMOVE_SECTOR_VISIBILITY_MASK = ~((int)VisibilityStateFlag.HexSectorVisible);

        public void Execute()
        {
            var invertedTriangleHeight = 1f / TriangleHeight;

            for (var i = 0; i < Filter.length; i++)
            {
                var entity = Filter[i];
                var worldPos = Positions.Get(entity).Value;
                var hexCoord = HexMath.DefineHex(worldPos.xz, HexEdgeLength);
                ref var visibilityState = ref VisibilityStates.Get(entity);

                if (!SectorVisibilityMasks.TryGetValue(hexCoord, out var sectorVisibilityMask))
                {
                    visibilityState.Value = (VisibilityStateFlag)(visibilityState.IntValue & REMOVE_SECTOR_VISIBILITY_MASK);
                    continue;
                }

                var tripos = TriangularMath.WorldToTrianglePosInvertedHeight(worldPos, invertedTriangleHeight);
                var sector = (int)HexMath.DefineSectorByTripos(tripos, HexEdgeLength, HexRadius, TriangleHeight);
                if ((sectorVisibilityMask & (1 << sector)) == 0)
                    visibilityState.Value = (VisibilityStateFlag)(visibilityState.IntValue & REMOVE_SECTOR_VISIBILITY_MASK);
                else
                    visibilityState.Value |= VisibilityStateFlag.HexSectorVisible;              
            }
        }
    }
}
