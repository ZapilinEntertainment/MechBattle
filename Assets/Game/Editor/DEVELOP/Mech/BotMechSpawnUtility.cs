using Unity.Mathematics;
using UnityEngine;
using TriInspector;
using VContainer;
using ZE.MechBattle.Ecs;
using System.Threading.Tasks;
using System.Threading;
using Scellecs.Morpeh;
using System;

namespace ZE.MechBattle.Develop
{
    public class BotMechSpawnUtility : MonoBehaviour
    {
        [Serializable]
        public struct SpawnData
        {
            public bool IsActive;
            public int PlayerId;
            public RigidTransform SpawnPoint;
            public StateKey StateKey;
            public float3 MoveTarget;
        }

        [SerializeField] private SpawnData[] _spawnData;
        private MechCreateRequestsFactory _mechRequestsFactory;
        private CreationRequestsHandler _requestsHandler;
        private StatesApplier _statesApplier;
        private MoveTargetApplier _moveTargetApplier;

        [Inject]
        public void Inject(
            MechCreateRequestsFactory mechCreateRequestsFactory, 
            CreationRequestsHandler requestsHandler, 
            StatesApplier statesApplier,
            MoveTargetApplier moveTargetApplier)
        {
            _mechRequestsFactory = mechCreateRequestsFactory;
            _requestsHandler = requestsHandler;
            _statesApplier = statesApplier;
            _moveTargetApplier = moveTargetApplier;
        }

        [Button, EnableInPlayMode]
        private async Task Spawn()
        {
            foreach (var spawnData in _spawnData)
            {
                if (!spawnData.IsActive)
                    continue;
                var request = new MechInstanceRequestComponent(
               new PlayerKey(spawnData.PlayerId),
               spawnData.SpawnPoint.pos,
               spawnData.SpawnPoint.rot,
               false);
                var mechCreationRequest = _mechRequestsFactory.CreateRequest(request);
                var mechEntity = await _requestsHandler.WaitUntilRequestCompleted(mechCreationRequest, CancellationToken.None);
                if (spawnData.StateKey == StateKey.Move)
                {
                    _moveTargetApplier.SetMoveTarget(mechEntity, spawnData.MoveTarget);
                }
            }           
        }
    }
}
