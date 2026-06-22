using System;
using UnityEngine;
using TriInspector;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Develop
{
    public class PortalPathsTableview : MonoBehaviour
    {
        [Serializable]
        public struct PortalData
        {
            public int Id;
            public int[] Portals;
        }

        [SerializeField, ReadOnly] private PortalData[] _serializedPortalsData;
        private IPortalPaths _portalPaths;

        [Inject]
        public void Inject(IPortalPaths portalPaths)
        {
            _portalPaths = portalPaths;
        }

        [Button("UpdateList")]
        private void UpdateList()
        {
            var count = _portalPaths.Count;
            _serializedPortalsData = new PortalData[count];
            var i = 0;
            foreach (var portalPathKvp in _portalPaths)
            {
                _serializedPortalsData[i++] = new PortalData()
                {
                    Id = portalPathKvp.Key,
                    Portals = portalPathKvp.Value.Points
                };
            }
        }
    
    }
}
