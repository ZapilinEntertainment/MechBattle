using UnityEngine;
using UnityEditor;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;


namespace ZE.MechBattle.Develop
{
    public class MovementOccupationCellsDrawer : MonoBehaviour
    {
        private bool _isInitialized = false;
        private INavigationMap _map;
        private MovementCellsList _movementCells; 

        [Inject]
        public void Inject(INavigationMap map, MovementCellsList movementCells)
        {
            _map = map;
            _movementCells = movementCells;
            _isInitialized = true;
        }

        public void OnDrawGizmosSelected()
        {
            if (!(enabled & _isInitialized))
                return;

            Handles.color = Color.yellow;
            foreach (var position in _movementCells.Keys)
            {
                var drawVertices = TrianglesDrawHelper.GetDrawVertices(position, _map);
                TrianglesDrawHelper.DrawHandles(drawVertices, false);
            }
        }

    }
}

