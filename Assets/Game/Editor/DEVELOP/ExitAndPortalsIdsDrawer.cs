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
        [SerializeField] private bool _drawPortalIds = true;
        [SerializeField] private bool _drawExitIds = true;

        private IHexPortalsList _portals;
        private IPortalExitsList _exits;
        private IPortalsLogic _portalLogic;
        private INavigationMap _map;
        private List<(Vector3 pos, int id)> _ids = new();


        [Inject]
        public void Inject(IPortalExitsList exitsList, INavigationMap map, IHexPortalsList portals, IPortalsLogic portalsLogic)
        {
            _portals = portals;
            _exits = exitsList;
            _map = map;
            _portalLogic = portalsLogic;
        }

        [Button("Update lists")]
        private void UpdateExitsAndPortalsList()
        {
            _ids.Clear();
            if (_drawExitIds) 
            { 
                foreach (var exitKvp in _exits)
                {
                    var centerPos = TriangularMath.TriangularToWorld(exitKvp.Value.Center, _map.TriangleHeight);
                    _ids.Add((centerPos, exitKvp.Key));
                }
            }

            if (_drawPortalIds)
            {
                foreach (var portalKvp in _portals)
                {
                    var triCenter = _portalLogic.GetPortalCenterTriangular(portalKvp.Key);
                    _ids.Add((TriangularMath.TriangularToWorld(triCenter, _map.TriangleHeight), portalKvp.Key));
                }
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
