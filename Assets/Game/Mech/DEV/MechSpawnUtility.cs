using UnityEngine;
using Unity.Mathematics;
using VContainer;
using TriInspector;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using System;

namespace ZE.MechBattle
{
    [Obsolete]
    public class MechSpawnUtility : MonoBehaviour
    {
        [SerializeField] private float3 _position;
        [SerializeField] private float3 _rotationDegrees;
        [SerializeField] private int _playerId;

        [SerializeField] private bool _assumingDirectControl = false;
        [SerializeField, DisableIf(nameof(IsUnderDirectControl))] private MechInputComponent _input;
        private bool _injected = false;
        private MechCreateRequestsFactory _requestsFactory;
        private Filter _mechFilter;
        private Stash<MechInputComponent> _inputComponents;

        [Inject]
        public void Inject(MechCreateRequestsFactory requestsFactory, World world)
        {
            _requestsFactory = requestsFactory;
            _mechFilter = world.Filter.With<MechComponent>().Build();
            _inputComponents = world.GetStash<MechInputComponent>();

            _injected = true;
        }

        [Button, EnableInPlayMode]
        private void SpawnMech()
        {
            var playerKey = new PlayerKey(_playerId);
            var rotation = quaternion.Euler(math.radians(_rotationDegrees));
            _requestsFactory.CreateRequest(new(playerKey, _position, rotation, true));
        }

        private void Update()
        {
            if (!_injected)
                return;

            MechInputComponent inputComponent;

            if (_assumingDirectControl)
            {
                var steer = Input.GetAxisRaw("Horizontal");
                var speed = Input.GetAxisRaw("Vertical");

                _input = new MechInputComponent()
                {
                    SpeedValue = speed,
                    SteerValue = steer
                };                
            }
            else
            {
                inputComponent = _input;
            }

            foreach (var mech in _mechFilter)
            {
                _inputComponents.Set(mech, _input);
            }
        }

        private bool IsUnderDirectControl() => _assumingDirectControl;
    }
}
