using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;
using ZE.MechBattle.Ecs;
using UnityEngine;
using Unity.Mathematics;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class EntityRotationsTester : MonoBehaviour
    {
        private class MonoView: IMonoView
        {
            private readonly GameObject _gameObject;
            private readonly Transform _transform;
            public Transform Transform => _transform;

            public string name { get => _gameObject.name; set => _gameObject.name = value;  }

            public MonoView(GameObject go)
            {
                _gameObject = go;
                _transform = _gameObject.transform;
            }

            public void Dispose() { }

            public void SetParent(Transform parent) => _transform.SetParent(parent, false);
        }

        [SerializeField] private float3 _localRotationTarget;
        [SerializeField] private float3 _transitRotationTarget;
        [SerializeField] private float _rotationSpeedDegrees = 15f;       
        [SerializeField] private GameObject _parent;
        [SerializeField] private GameObject _child;

        private ViewSynchronizationApplier _viewSyncApplier;
        private ParentingRelationsApplier _parentingRelationsApplier;
        private TransformAspectHandler _transformAspectHandler;
        private Entity _parentEntity;
        private Entity _childEntity;
        private Entity _transitEntity;
        [ShowInInspector] private int ParentId => _parentEntity.Id;
        [ShowInInspector] private int ChildId => _childEntity.Id;

        private Stash<LocalTargetRotationComponent> _localRotationTargetStash;
        private Stash<RotationSpeedComponent> _rotationSpeedStash;

        private quaternion TargetLocalRotation => Quaternion.Euler(_localRotationTarget);

        [Inject]
        public void Inject(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            ParentingRelationsApplier parentingRelationsApplier,
            TransformAspectHandler transformAspectHandler)
        {
            _viewSyncApplier = viewSyncApplier;
            _parentingRelationsApplier = parentingRelationsApplier;
            _transformAspectHandler = transformAspectHandler;

            _localRotationTargetStash = world.GetStash<LocalTargetRotationComponent>();
            _rotationSpeedStash = world.GetStash<RotationSpeedComponent>();


            var parentView = new MonoView(_parent);
            _parentEntity = world.CreateEntity();
            _viewSyncApplier.Apply(_parentEntity, parentView, applyViewPosition: false);

            _transitEntity = world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = _parentEntity,
                ChildEntity = _transitEntity,
                LocalPos = float3.zero,
                LocalRot = quaternion.identity
            });
            _rotationSpeedStash.Set(_transitEntity, new(math.radians(_rotationSpeedDegrees)));

            var childView = new MonoView(_child);
            _childEntity = world.CreateEntity();
            _viewSyncApplier.Apply(_childEntity, childView, applyViewPosition: false);
            _rotationSpeedStash.Set(_childEntity, new(math.radians(_rotationSpeedDegrees)));


            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = _transitEntity,
                ChildEntity = _childEntity,
                LocalPos = float3.zero,
                LocalRot = TargetLocalRotation
            });

            
        }

        public void SetParent(Transform parent) => transform.SetParent(parent, false);

        [Button(nameof(UpdateTargetRotation))]
        private void UpdateTargetRotation()
        {
            _localRotationTargetStash.Set(_childEntity, new() { Value = TargetLocalRotation});
            _localRotationTargetStash.Set(_transitEntity, new() { Value = Quaternion.Euler(_transitRotationTarget) });
        }

        [Button(nameof(UpdateTransforms))]
        private void UpdateTransforms()
        {
            _transformAspectHandler.ApplyViewPositionToEntity(_parentEntity, _parent.transform);
        }
    }
}
