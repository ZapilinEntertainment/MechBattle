using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class DEV_UnitsMoveCommandScript : MonoBehaviour
    {
        [SerializeField] private KeyCode _commandKey = KeyCode.M;
        [SerializeField] private Vector3 _center = Vector3.zero;
        [SerializeField] private float _radius = 100f;
        private Filter _filter;
        private Stash<MoveTargetComponent> _moveTargets;
        private Vector3? _targetPos;

        [Inject]
        public void Inject(World world)
        {
            _filter = world.Filter.With<StateComponent>().Build();
            _moveTargets = world.GetStash<MoveTargetComponent>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(_commandKey))
            {
                var random = Random.insideUnitCircle;
                var pos = new Vector3(random.x * _radius, 0f, random.y * _radius) + _center;
                _targetPos = pos;

                foreach (var entity in _filter)
                {
                    _moveTargets.Set(entity, new() { Value = pos });
                }
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
