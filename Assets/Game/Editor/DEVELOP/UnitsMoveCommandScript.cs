using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using TriInspector;
using Unity.Mathematics;

namespace ZE.MechBattle.Develop
{
    public class UnitsMoveCommandScript : MonoBehaviour
    {
        [SerializeField] private Vector3 _center = Vector3.zero;
        [SerializeField] private float _radius = 100f;
        [SerializeField] private Vector3 _manualDefinedTarget;
        private Filter _filter;
        private Stash<MoveTargetComponent> _moveTargets;
        private Vector3? _targetPos;
        private float _triangleHeight;

        [Inject]
        public void Inject(World world, INavigationMap map)
        {
            _filter = world.Filter.With<NavigationAgentComponent>().Build();
            _moveTargets = world.GetStash<MoveTargetComponent>();
            _triangleHeight = map.TriangleHeight;
        }

        [Button("Set new random target")]
        private void SetNewRandomTarget()
        {
            var random = UnityEngine.Random.insideUnitCircle;
            var pos = new Vector3(random.x * _radius, 0f, random.y * _radius) + _center;
            _targetPos = pos;
            _manualDefinedTarget = pos;

            SetEntitiesTarget(pos);
        }

        [Button("Set manual target")]
        private void SetManualTarget() => SetEntitiesTarget(_manualDefinedTarget);

        [Button("Stop all movement")]
        private void StopAllMovement()
        {
            foreach (var entity in _filter)
            {
                _moveTargets.Remove(entity);
            }
        }

        private void SetEntitiesTarget(float3 pos)
        {
            foreach (var entity in _filter)
            {
                _moveTargets.Set(entity, new(pos, _triangleHeight));
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_targetPos == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_targetPos.Value, 5f);
        }
#endif
    }
}
