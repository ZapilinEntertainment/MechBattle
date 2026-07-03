using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public interface INavigationGridHandler
    {
        bool IsCellOccupied(IntTriangularPos pos);
    }
    public class NavigationGridHandler : INavigationGridHandler
    {
        private readonly MovementCellsList _movementCellsList;

        [Inject]
        public NavigationGridHandler(MovementCellsList movementCellsList)
        {
            _movementCellsList = movementCellsList;
        }

        public bool IsCellOccupied(IntTriangularPos pos) => _movementCellsList.TryGetValue(pos, out var cellValue) && cellValue.ProjectionStepIndex == 0;
    }
}
