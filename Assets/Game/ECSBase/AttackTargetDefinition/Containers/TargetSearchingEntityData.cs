using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public readonly struct TargetSearchingEntityData
    {
        public readonly int EntityDataIndex;
        public readonly float SearchRadiusInUnits;
        public readonly float MinRange;
        public readonly int SearchRadiusInTriangles;
        public readonly bool UseLongRangeSearch;

        public TargetSearchingEntityData(
            int entityDataIndex, 
            float searchRadiusInUnits, 
            int searchRadiusInTris, 
            bool useLongRangeSearch,
             float minRange)
        {
            EntityDataIndex = entityDataIndex;
            SearchRadiusInUnits = searchRadiusInUnits;
            SearchRadiusInTriangles = searchRadiusInTris;
            UseLongRangeSearch = useLongRangeSearch;
            MinRange = minRange;
        }
    }
}
