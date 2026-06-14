using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface ICellDataProvider<CellHeightData> where CellHeightData : unmanaged, ICellHeightData
    {
        bool TryGetCellData(IntTriangularPos pos, out TriangleCellData<CellHeightData> cellData);
    }

    public readonly struct UpdateCellNeighboursMaskLogic<HeightData, CellDataList>
         where HeightData : unmanaged, ICellHeightData
         where CellDataList : ICellDataProvider<HeightData>
    {
        private readonly bool _isValid;
        private readonly TriangleCellData<HeightData> _cellData;
        private readonly CellDataList _cellDataList;
        private readonly float _maxElevationDifference;

        public UpdateCellNeighboursMaskLogic(IntTriangularPos cellPos, CellDataList cellDataList, float maxElevationDifference)
        {
            _isValid =  cellDataList.TryGetCellData(cellPos, out _cellData);
            _cellDataList = cellDataList;
            _maxElevationDifference = maxElevationDifference;
        }

        public int CalculateNeighboursMask()
        {
            if (!_isValid)
                return 0;

            var neighboursAccessMask = 0;
            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                var neighbourPos = TriangularMath.GetNeighbourByDirection(_cellData.Tripos, i);
                if (!_cellDataList.TryGetCellData(neighbourPos, out var neighbourData) 
                    || TrianglesTransitionLogic.IsCloseTransitionPossible<HeightData>(_cellData, neighbourData, _maxElevationDifference))
                    continue;
                neighboursAccessMask |= (1 << i);
            }

            return TrianglesTransitionLogic.CheckMaskForJumpNeighbours(neighboursAccessMask, _cellData.Tripos.IsPeak);
        }
    }
}
