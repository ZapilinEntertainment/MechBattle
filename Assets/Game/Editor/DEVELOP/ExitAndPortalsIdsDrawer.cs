using System.Collections.Generic;
using ZE.MechBattle.Navigation;
using VContainer;
using UnityEngine;
using UnityEditor;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class ExitAndPortalsIdsDrawer : MonoBehaviour
    {
        private IHexPortalsList _portals;
        private IPortalExitsList _exits;
        private INavigationMap _map;
        private List<(Vector3 pos, int id)> _ids = new();


        [Inject]
        public void Inject(IPortalExitsList exitsList, INavigationMap map, IHexPortalsList portals)
        {
            _portals = portals;
            _exits = exitsList;
            _map = map;
        }

        [Button("Update lists")]
        private void UpdateExitsAndPortalsList()
        {
            _ids.Clear();
            foreach (var exitKvp in _exits)
            {
                var centerPos = TriangularMath.TriangularToWorld(exitKvp.Value.Center, _map.TriangleHeight);
                _ids.Add((centerPos, exitKvp.Key));
            }

            foreach (var portalKvp in _portals)
            {
                _exits.TryGetValue(portalKvp.Value.ExitIdA, out var exitA);
                _exits.TryGetValue(portalKvp.Value.ExitIdB, out var exitB);

                var centerA = TriangularMath.TriangularToWorld(exitA.Center, _map.TriangleHeight);
                var centerB = TriangularMath.TriangularToWorld(exitB.Center, _map.TriangleHeight);

                _ids.Add((Vector3.Lerp(centerA, centerB, 0.5f), portalKvp.Key));
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            foreach (var idData in _ids)
            {
                Handles.Label(idData.pos, idData.id.ToString());
            }
        }
    }
}
