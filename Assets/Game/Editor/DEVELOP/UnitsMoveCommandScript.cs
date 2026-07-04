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
        private Stash<ChangeMoveTargetRequestComponent> _moveTargets;
        private Vector3? _targetPos;
        private float _triangleHeight;
        private float _hexEdgeLength;

        [Inject]
        public void Inject(World world, INavigationMap map)
        {
            _filter = world.Filter.With<NavigationAgentComponent>().Build();
            _moveTargets = world.GetStash<ChangeMoveTargetRequestComponent>();

            _triangleHeight = map.TriangleHeight;
            _hexEdgeLength = map.HexEdgeLength;
        }

        [EnableInPlayMode, Button("Set new random target")]
        private void SetNewRandomTarget()
        {
            var random = UnityEngine.Random.insideUnitCircle;
            var pos = new Vector3(random.x * _radius, 0f, random.y * _radius) + _center;
            _targetPos = pos;
            _manualDefinedTarget = pos;

            SetEntitiesTarget(pos);
        }

        [EnableInPlayMode, Button("Set manual target")]
        private void SetManualTarget() => SetEntitiesTarget(_manualDefinedTarget);

        [EnableInPlayMode, Button("Stop all movement")]
        private void StopAllMovement()
        {
            foreach (var entity in _filter)
            {
                _moveTargets.Remove(entity);
            }
        }

        [EnableInPlayMode, Button("Move to object")]
        private void MoveToObject() => SetEntitiesTarget(transform.position);

        private void SetEntitiesTarget(float3 pos)
        {
            var tripos = TriangularMath.WorldToTrianglePos(pos, _triangleHeight);
            var hexCoord = HexMath.DefineHex(pos.xz, _hexEdgeLength);

            foreach (var entity in _filter)
            {
                
                _moveTargets.Set(entity, new(pos, tripos, hexCoord));
            }
            UnityEngine.Debug.Log($"move target set to {pos}");
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
