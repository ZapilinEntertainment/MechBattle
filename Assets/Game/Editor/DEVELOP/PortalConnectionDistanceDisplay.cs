using UnityEngine;
using TriInspector;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class PortalConnectionDistanceDisplay : MonoBehaviour
    {
        [SerializeField] private int _portalIdA;
        [SerializeField] private int _portalIdB;
        [SerializeField, ReadOnly] private float _distance;
        private IHexPortalsList _portalsList;
        private IPortalConnectionsList _connectionsList;

        [Inject]
        public void Inject(IHexPortalsList portals, IPortalConnectionsList portalConnections)
        {
            _portalsList = portals;
            _connectionsList = portalConnections;
        }

        [Button("Display")]
        private void Display()
        {
            if (!_portalsList.TryGetValue(_portalIdA, out var portalA))
            {
                _portalIdA = -1;
                return;
            }

            if (!_portalsList.TryGetValue(_portalIdB, out var portalB))
            {
                _portalIdB = -1;
                return;
            }

            _connectionsList.TryGetDistance(_portalIdA, _portalIdB, out _distance);
        }
    }
}
