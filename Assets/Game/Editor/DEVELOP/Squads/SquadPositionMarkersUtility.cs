using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Develop
{
    public class SquadPositionMarkersUtility : MonoBehaviour
    {
        [SerializeField] private float _markerSize = 5f;
        [SerializeField] private Color _color = Color.indianRed;
        private SquadHandler _squadHandler;

        [Inject]
        public void Inject(SquadHandler squadHandler)
        {
            _squadHandler = squadHandler;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || _squadHandler == null)
                return;

            Gizmos.color = _color;
            foreach (var squadEntity in _squadHandler.GetNextSquad())
            {
                Gizmos.DrawSphere(_squadHandler.GetSquadPosition(squadEntity), _markerSize);
            }            
        }
    }
}
