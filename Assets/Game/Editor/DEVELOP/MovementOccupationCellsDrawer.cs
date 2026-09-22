using UnityEngine;
using UnityEditor;
using Unity.Collections;
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
        private Filter _cellsFilter;
        private Stash<CellEntityComponent> _cellComponents;

        [Inject]
        public void Inject(INavigationMap map, World world)
        {
            _map = map;
            _cellsFilter = world.Filter.With<CellMovementDataComponent>().Build();
            _cellComponents = world.GetStash<CellEntityComponent>();

            _isInitialized = true;
        }

        public void OnDrawGizmosSelected()
        {
            if (!enabled || !_isInitialized || _cellsFilter.IsEmpty())
                return;

            Handles.color = Color.yellow;
            foreach (var cellEntity in _cellsFilter)
            {
                var tripos = _cellComponents.Get(cellEntity).Tripos;
                var drawVertices = TrianglesDrawHelper.GetDrawVertices(tripos, _map);
                TrianglesDrawHelper.DrawHandles(drawVertices, false);
            }
        }

    }
}

