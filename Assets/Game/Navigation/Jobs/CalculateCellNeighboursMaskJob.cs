using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct CalculateCellNeighboursMaskJob : IJobParallelFor
    {
        [BurstCompile]
        public struct JobCellDataProvider : ICellDataProvider<CellHeightData>
        {
            private NativeArray<CellHeightData>.ReadOnly _heightData;
            private FlattenedHexList<CellPassabilityData> _passabilityData;

            public IntTriangularPos IndexToTriangular(int index) => _passabilityData.IndexToTriangular(index);
            public bool IsCellPassable(int index) => _passabilityData[index].IsPassable;
            public CellHeightData GetHeightData(int index) => _heightData[index];

            public JobCellDataProvider(
                NativeArray<CellHeightData>.ReadOnly raycastData, 
                FlattenedHexList<CellPassabilityData> passabilityData)
            {
                _heightData = raycastData;
                _passabilityData = passabilityData;
            }

            public JobCellDataProvider ChangePassabilityData(in FlattenedHexList<CellPassabilityData> passData) =>
                new(_heightData, passData);

            public bool TryGetCellData(IntTriangularPos pos, out TriangleCellData<CellHeightData> cellData)
            {
                if (!_passabilityData.TryGetIndex(pos, out var index))
                {
                    //UnityEngine.Debug.Log($"{pos} not recognized");
                    cellData = default;
                    return false;
                }
                
                cellData = GetCellData(index, pos);
                return true;
            }

            public TriangleCellData<CellHeightData> GetCellData(int index, IntTriangularPos pos) => new(pos, IsCellPassable(index), GetHeightData(index));

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
            var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, JobCellDataProvider>(cellTripos, CellDataProvider, MaxElevationDifference);
            var neighboursMask = logic.CalculateNeighboursMask();
            CellDataProvider.SetNeighboursMask(index, neighboursMask);
        }
    }
}
