using UnityEngine;
using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class UnitAimTester : MonoBehaviour
    {
        public enum TargetEntity : byte { Unit, Weapon, Tower, Barrel};

        [SerializeField] private string _unitId;
        [Space]
        [SerializeField] private TargetEntity _changingEntity;
        [SerializeField] private float3 _targetRotation;
        [SerializeField] private Transform _target;
 
        private bool _isTargeting = false;
        private World _world;
        private TransformAspectHandler _transformHandler;
        private UnitsFactory _unitsFactory;
        private ViewSynchronizationApplier _viewSyncApplier;

        private Stash<LocalTargetRotationComponent> _localTargetRotationStash;
        private Stash<RotationSpeedComponent> _rotationSpeedStash;
        private Stash<TransformUpdatedTag> _transformUpdateTags;
        private Stash<EntityDisposeTag> _disposeTags;

        private Stash<WeaponTowerComponent> _weaponTowerComponents;
        private Stash<WeaponBarrelComponent> _weaponBarrelComponents;
        private Stash<StateComponent> _stateComponents;
        private Stash<ParentEntityComponent> _parentEntities;
        private Stash<TargetSearchRadiusComponent> _targetSearchRadiusComponents;

        private Stash<AttackTargetComponent> _attackTargetComponents;
        private Stash<PositionComponent> _positionComponents;
        private Stash<AttackRangeReachedTag> _attackRangeReachedTag;

        [ShowInInspector, ReadOnly] private Entity _unitEntity;
        [ShowInInspector, ReadOnly] private Entity _weaponEntity;
        [ShowInInspector, ReadOnly] private Entity _towerEntity;
        [ShowInInspector, ReadOnly] private Entity _barrelEntity;
        private Entity _targetEntity;

        [Inject]
        public void Inject(
            World world, 
            UnitsFactory unitsFactory,
            TransformAspectHandler transformAspectHandler,
            ViewSynchronizationApplier viewSyncApplier)
        {
            _world = world;
            _transformHandler = transformAspectHandler;
            _unitsFactory = unitsFactory;
            _viewSyncApplier = viewSyncApplier;

            _localTargetRotationStash = world.GetStash<LocalTargetRotationComponent>();
            _rotationSpeedStash = world.GetStash<RotationSpeedComponent>();
            _transformUpdateTags = world.GetStash<TransformUpdatedTag>();
            _disposeTags = world.GetStash<EntityDisposeTag>();

            _weaponTowerComponents = world.GetStash<WeaponTowerComponent>();
            _weaponBarrelComponents = world.GetStash<WeaponBarrelComponent>();
            _stateComponents = world.GetStash<StateComponent>();
            _parentEntities = world.GetStash<ParentEntityComponent>();
            _targetSearchRadiusComponents = world.GetStash<TargetSearchRadiusComponent>();

            _attackTargetComponents = world.GetStash<AttackTargetComponent>();
            _positionComponents = world.GetStash<PositionComponent>();
            _attackRangeReachedTag = world.GetStash<AttackRangeReachedTag>();
        }

        private void Start()
        {
            _unitEntity = _unitsFactory.Build(_unitId, new(transform.rotation, transform.position));
             _world.Commit();

            var filter = _world.Filter.With<ParentEntityComponent>().Build();
            foreach (var entity in filter)
            {
                var parentEntity = _parentEntities.Get(entity).Value;
                if (parentEntity == _unitEntity)
                {
                    _weaponEntity = entity;
                    break;
                }
            }

            _towerEntity = _weaponTowerComponents.Get(_weaponEntity).TowerEntity;
            _barrelEntity = _weaponBarrelComponents.Get(_weaponEntity).BarrelEntity;

            // remove state component to control aim correctly
            _stateComponents.Remove(_unitEntity);
            _targetSearchRadiusComponents.Remove(_unitEntity);

            //
            _targetEntity = _world.CreateEntity();
            _viewSyncApplier.Apply(_targetEntity, new ViewPartContainer(_target), applyViewPosition: false);
        }

        private void Update()
        {
            if (_isTargeting)
            {
                _positionComponents.Set(_targetEntity, new() { Value = _target.position});
            }
        }

        [Button(nameof(ApplyTargetRotation)), EnableInPlayMode]
        private void ApplyTargetRotation()
        {
            _isTargeting = false;
            _attackTargetComponents.Remove(_unitEntity);


            Entity targetEntity;
            switch (_changingEntity)
            {
                case TargetEntity.Weapon: targetEntity = _weaponEntity; break;
                    case TargetEntity.Tower: targetEntity = _towerEntity; break;
                    case TargetEntity.Barrel: targetEntity = _barrelEntity; break;
                default: targetEntity = _unitEntity; break;
            }

            var quaternion = Quaternion.Euler(_targetRotation);
            if (_rotationSpeedStash.Has(targetEntity))
            {
                _localTargetRotationStash.Set(targetEntity, new() { Value = quaternion });
            }                
            else
            {
                _transformHandler.SetLocalRotation(targetEntity, quaternion);
            }
            _world.Commit();
        }

        [Button(nameof(UpdateTransforms)), EnableInPlayMode]
        private void UpdateTransforms()
        {
            _transformUpdateTags.Set(_unitEntity);
        }

        [Button(nameof(ResetTank)), EnableInPlayMode]
        private void ResetTank()
        {
            _isTargeting = false;
            _disposeTags.Set(_unitEntity);
            Start();
        }


        // TODO: if target entity is out of range, tank won't respond, need warning

        [Button(nameof(AssignTarget)), EnableInPlayMode]
        private void AssignTarget()
        {
            _attackTargetComponents.Set(_unitEntity, new() { Entity = _targetEntity});
            _isTargeting = true;
        }
    }
}
