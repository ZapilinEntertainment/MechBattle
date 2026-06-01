using UnityEngine;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Navigation.DebugOverlay;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class MapPassabilityDebug : MonoBehaviour
    {
        private INavigationMap _map;
        private MapPassabilityDrawer _drawer;
        [SerializeField, ReadOnly] private int _lastDrawnVersion = -1;

        [Inject]
        public void Inject(INavigationMap map)
        {
            _map = map;
            _drawer = new();
        }

        private void Update()
        {
            if (_map.Version != _lastDrawnVersion)
            {
                _drawer.RedrawMap(_map);
                _lastDrawnVersion = _map.Version;
            }
        }

        private void OnDrawGizmos()
        {
            if (_map == null || !enabled)
                return;

            _drawer.DrawHandles();
        }
    }
}
