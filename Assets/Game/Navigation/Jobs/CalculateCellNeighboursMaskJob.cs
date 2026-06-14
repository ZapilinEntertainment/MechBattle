using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct CalculateCellNeighboursMaskJob : IJobParallelFor
    {
        [BurstCompile]
        public struct JobCellDataProvider : ICellDataProvider<RefinedTriangleRaycastData>
        {
            private NativeArray<RefinedTriangleRaycastData>.ReadOnly _raycastData;
            private FlattenedHexList<CellPassabilityData> _passabilityData;

            public IntTriangularPos IndexToTriangular(int index) => _passabilityData.IndexToTriangular(index);
            public bool IsCellPassable(int index) => _passabilityData[index].IsPassable;
            public RefinedTriangleRaycastData GetHeightData(int index) => _raycastData[index];

            public JobCellDataProvider(
                NativeArray<RefinedTriangleRaycastData>.ReadOnly raycastData, 
                FlattenedHexList<CellPassabilityData> passabilityData)
            {
                _raycastData = raycastData;
                _passabilityData = passabilityData;
            }

            public bool TryGetCellData(IntTriangularPos pos, out TriangleCellData<RefinedTriangleRaycastData> cellData)
            {
                if (!_passabilityData.TryGetIndex(pos, out var index))
                {
                    cellData = default;
                    return false;
                }
                
                cellData = GetCellData(index, pos);
                return true;
            }

            public TriangleCellData<RefinedTriangleRaycastData> GetCellData(int index, IntTriangularPos pos) => new(pos, IsCellPassable(index), GetHeightData(index));

            public void SetNeighboursMask(int index, int mask)
            {
                var data = _passabilityData[index];
                data.NeighboursMask = mask;
                _passabilityData[index] = data;
            }
        }

        public JobCellDataProvider CellDataProvider;
        public float MaxElevationDifference;

        public void Execute(int index)
        {
            var cellTripos = CellDataProvider.IndexToTriangular(index);
            var logic = new UpdateCellNeighboursMaskLogic<RefinedTriangleRaycastData, JobCellDataProvider>(cellTripos, CellDataProvider, MaxElevationDifference);
            var neighboursMask = logic.CalculateNeighboursMask();
            CellDataProvider.SetNeighboursMask(index, neighboursMask);
        }
    }
}
