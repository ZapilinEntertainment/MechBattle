using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class NavigationGridHandler
    {
        private readonly IMovementCellsMap _movementCellsList;

        [Inject]
        public NavigationGridHandler(IMovementCellsMap movementCellsList)
        {
            _movementCellsList = movementCellsList;
        }

        public bool IsCellOccupied(IntTriangularPos pos) => _movementCellsList.TryGetValue(pos, out var cellValue) && cellValue.ProjectionStepIndex == 0;

    }
}
