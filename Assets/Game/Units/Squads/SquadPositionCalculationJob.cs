using Scellecs.Morpeh.Native;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct SquadPositionCalculationJob : IJob
    {
        [ReadOnly] public NativeFilter SquadMembersFilter;
        [ReadOnly] public NativeFilter SquadsFilter;
        [ReadOnly] public NativeStash<SquadMemberComponent> SquadMembers;
        [ReadOnly] public NativeStash<SquadComponent> SquadComponents;
        public NativeStash<PositionComponent> Positions;
        
        private readonly struct SquadCoordinate : IComparable<SquadCoordinate>
        {
            public readonly float Coordinate;
            public readonly int SquadId;

            public SquadCoordinate(float coord, int squadId)
            {
                Coordinate = coord;
                SquadId = squadId;
            }

            public int CompareTo(SquadCoordinate other) =>
                Coordinate.CompareTo(other.Coordinate);
        }

        public void Execute()
        {
            var unitsCount = SquadMembersFilter.length;

            var xs = new NativeArray<SquadCoordinate>(unitsCount, Allocator.Temp);
            var zs = new NativeArray<SquadCoordinate>(unitsCount, Allocator.Temp);

            for (var i = 0; i < unitsCount; i++)
            {
                var unitEntity = SquadMembersFilter[i];
                var position = Positions.Get(unitEntity).Value;
                var squadId = SquadMembers.Get(unitEntity).SquadEntity.Id;

                xs[i] = new(position.x, squadId);
                zs[i] = new(position.z, squadId);
            }

            xs.Sort();
            zs.Sort();

            for (var i = 0; i < SquadsFilter.length; i++)
            {
                var squadEntity = SquadsFilter[i];
                var squadComponent = SquadComponents.Get(squadEntity);
                var squadId = squadEntity.Id;
                var medianIndex = squadComponent.MembersCount / 2;
                var xs0 = 0f;
                var xs1 = 0f;
                var zs0 = 0f;
                var zs1 = 0f;

                var arrayIndex = 0;
                var searchMedianX = true;
                var searchMedianZ = true;
                var indexX = 0;
                var indexZ = 0;

                while (arrayIndex < unitsCount && (searchMedianX | searchMedianZ))
                {
                    var coordX = xs[arrayIndex];
                    var coordZ = zs[arrayIndex];

                    if (searchMedianX && coordX.SquadId == squadId)
                    {
                        xs0 = xs1;
                        xs1 = coordX.Coordinate;
                        searchMedianX = !(indexX == medianIndex);

                        indexX++;
                    }

                    if (searchMedianZ && coordZ.SquadId == squadId)
                    {
                        zs0 = zs1;
                        zs1 = coordZ.Coordinate;
                        searchMedianZ = !(indexZ == medianIndex);

                        indexZ++;
                    }

                    arrayIndex++;
                }

                float3 medianPos;
                if ((squadComponent.MembersCount & 1) == 1)
                    medianPos = new float3(xs1, 0f, zs1);
                else
                    medianPos = new float3((xs0 + xs1) / 2f, 0f, (zs0 + zs1) / 2f);

                Positions.Get(squadEntity).Value = medianPos;
            }
        }
    }
}
