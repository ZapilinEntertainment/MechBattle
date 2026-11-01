using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using TriInspector;
using Unity.Mathematics;
using UnityEditor;

namespace ZE.MechBattle.Develop
{
    public class ArtilleryFireOrderUtility : MonoBehaviour
    {
        [SerializeField] private float3 _attackPosition;

        private Filter _artilleryFilter;
        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<PositionComponent> _positions;
        private Entity _targetEntity = default;
        private World _world;

        [Inject]
        public void Inject(World world)
        {
            _world = world;

            _artilleryFilter = _world.Filter.With<WeaponHighAngleAimTag>().Build();
            _attackTargets = _world.GetStash<AttackTargetComponent>();
            _positions = _world.GetStash<PositionComponent>();
        }

        [Button("Attack"), EnableInPlayMode]
        public void Attack()
        {
            if (_world.IsDisposed(_targetEntity))
            {
                _targetEntity = _world.CreateEntity();
            }
            _positions.Set(_targetEntity, new() { Value = _attackPosition });

            foreach (var entity in _artilleryFilter)
            {
                _attackTargets.Set(entity, new(_targetEntity));
            }
        }

        [Button("Stop"), EnableInPlayMode]
        public void Stop()
        {
            foreach (var entity in _artilleryFilter)
            {
                _attackTargets.Remove(entity);
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled)
                return;
            Handles.color = Color.greenYellow;
            Handles.DrawSolidDisc(_attackPosition, Vector3.up, 5f);
        }
    }
}
