using UnityEngine;
using Unity.Mathematics;
using VContainer;
using TriInspector;

namespace ZE.MechBattle
{
    public class MechSpawnUtility : MonoBehaviour
    {
        [SerializeField] private float3 _position;
        [SerializeField] private float3 _rotationDegrees;
        [SerializeField] private int _playerId;
        private MechCreateRequestsFactory _requestsFactory;

        [Inject]
        public void Inject(MechCreateRequestsFactory requestsFactory)
        {
            _requestsFactory = requestsFactory;
        }

        [Button, EnableInPlayMode]
        private void SpawnMech()
        {
            var playerKey = new PlayerKey(_playerId);
            var rotation = quaternion.Euler(math.radians(_rotationDegrees));
            _requestsFactory.CreateRequest(new(playerKey, _position, rotation));
        }
    }
}
