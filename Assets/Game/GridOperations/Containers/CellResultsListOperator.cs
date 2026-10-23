using System.Collections.Generic;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class CellResultsListOperator : AffectedTrianglesCalculator.IAffectedCellsOperator
    {
        public IReadOnlyList<IntTriangularPos> List => _list;
        private readonly List<IntTriangularPos> _list;

        public CellResultsListOperator(int defaultCapacity)
        {
            _list = new(defaultCapacity);
        }

        public void AddCell(IntTriangularPos tripos) => _list.Add(tripos);
        public void Clear() => _list.Clear();
    }
}
