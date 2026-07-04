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
        private NativeParallelHashMap<IntTriangularPos, CellMovementData>.ReadOnly _readonlyMap;

        [Inject]
        public void Inject(INavigationMap map, IMovementCellsMap movementCells)
        {
            _map = map;
            _readonlyMap = movementCells.AsReadonlyMap();
            _isInitialized = true;
        }

        public void OnDrawGizmosSelected()
        {
            if (!(enabled & _isInitialized & _readonlyMap.IsCreated))
                return;

            Handles.color = Color.yellow;
            foreach (var cellKvp in _readonlyMap)
            {
                var drawVertices = TrianglesDrawHelper.GetDrawVertices(cellKvp.Key, _map);
                TrianglesDrawHelper.DrawHandles(drawVertices, false);
            }
        }

    }
}

