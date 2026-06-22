using UnityEngine;
using UnityEditor;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;

namespace ZE.MechBattle.Develop
{
    public class EntityCellOccupationDrawer : MonoBehaviour
    {
        private bool _isInitialized = false;
        private Stash<TriangularPosComponent> _positions;
        private INavigationMap _map;

        [Inject]
        public void Inject(World world, INavigationMap map)
        {
            _map = map;
            _positions = world.GetStash<TriangularPosComponent>();
            _isInitialized = true;
        }

        public void OnDrawGizmosSelected()
        {
            if (!(enabled &_isInitialized))
                return;

            Handles.color = Color.white;
            foreach (var position in _positions)
            {
                var drawVertices = TrianglesDrawHelper.GetDrawVertices(position.Value, _map);
                TrianglesDrawHelper.DrawHandles(drawVertices, false);
            }
        }
    
    }
}
