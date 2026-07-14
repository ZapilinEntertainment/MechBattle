using UnityEngine;
using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using TriInspector;

namespace ZE.MechBattle.Develop
{
    public class TankAimTester : MonoBehaviour
    {
        public enum TargetEntity : byte { Tank, Weapon, Tower, Barrel};

        [SerializeField] private TankView _tankViewPrefab;
        [SerializeField] private UnitConfig _unitConfig;
        [Space]
        [SerializeField] private TargetEntity _changingEntity;
        [SerializeField] private float3 _targetRotation;
 
        private World _world;
        private ViewSynchronizationApplier _viewSyncApplier;
        private ParentingRelationsApplier _parentingRelationsApplier;
        private TransformAspectHandler _transformHandler;
        private Stash<LocalTargetRotationComponent> _localTargetRotationStash;
        private Stash<RotationSpeedComponent> _rotationSpeedStash;
        private Stash<TransformUpdatedTag> _transformUpdateTags;
        private Stash<EntityDisposeTag> _disposeTags;
        private Stash<DisposableViewComponent> _disposableViews;

        [ShowInInspector, ReadOnly] private Entity _tankEntity;
        [ShowInInspector, ReadOnly] private Entity _weaponEntity;
        [ShowInInspector, ReadOnly] private Entity _towerEntity;
        [ShowInInspector, ReadOnly] private Entity _barrelEntity;
 
        [Inject]
        public void Inject(
            World world, 
            ViewSynchronizationApplier viewSyncApplier, 
            ParentingRelationsApplier parentingRelationsApplier,
            TransformAspectHandler transformAspectHandler)
        {
            _world = world;
            _viewSyncApplier = viewSyncApplier;
            _parentingRelationsApplier = parentingRelationsApplier;
            _transformHandler = transformAspectHandler;

            _localTargetRotationStash = world.GetStash<LocalTargetRotationComponent>();
            _rotationSpeedStash = world.GetStash<RotationSpeedComponent>();
            _transformUpdateTags = world.GetStash<TransformUpdatedTag>();
            _disposeTags = world.GetStash<EntityDisposeTag>();
            _disposableViews = world.GetStash<DisposableViewComponent>();
        }

        private void Start()
        {
            var view = Instantiate(_tankViewPrefab, transform.position, transform.rotation);

            _tankEntity = _world.CreateEntity();
            _viewSyncApplier.Apply(_tankEntity, view);
            _disposableViews.Set(_tankEntity, new(view.gameObject));

            _unitConfig.TryGetWeaponData(out var weaponData);

            _weaponEntity = _world.CreateEntity();
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = _weaponEntity,
                ParentEntity = _tankEntity,
                LocalPos = weaponData.AttachmentProtocol.LocalPosition,
                LocalRot = weaponData.AttachmentProtocol.LocalRotation
            });

            weaponData.Config.TryGetTowerAttachmentProtocol(out var towerAttachmentProtocol);
            _towerEntity = _world.CreateEntity();
            view.TryGetPartByKey(new() { Type = ViewPartType.Tower}, out var towerView);
            _viewSyncApplier.Apply(_towerEntity, towerView);

            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = _towerEntity,
                ParentEntity = _weaponEntity,
                LocalPos = towerAttachmentProtocol.LocalPosition,
                LocalRot = quaternion.identity
            });


            weaponData.Config.TryGetBarrelAttachmentProtocol(out var barrelAttachmentProtocol);
            _barrelEntity = _world.CreateEntity();
            view.TryGetPartByKey(new() { Type = ViewPartType.Barrel }, out var barrelView);
            _viewSyncApplier.Apply(_barrelEntity, barrelView);
            _parentingRelationsApplier.Apply(new()
            {
                ChildEntity = _barrelEntity,
                ParentEntity = _towerEntity,
                LocalPos = barrelAttachmentProtocol.LocalPosition,
                LocalRot = quaternion.identity
            });

            // =======

            var rotationSpeed = math.radians( towerAttachmentProtocol.RotationSpeedDegrees);
            var barrelRotationSpeed = math.radians(barrelAttachmentProtocol.RotationSpeedDegrees);
            _rotationSpeedStash.Set(_towerEntity, new(rotationSpeed));
            _rotationSpeedStash.Set(_barrelEntity, new(barrelRotationSpeed));
        }

        [Button(nameof(ApplyTargetRotation)), EnableInPlayMode]
        private void ApplyTargetRotation()
        {
            Entity targetEntity;
            switch (_changingEntity)
            {
                case TargetEntity.Weapon: targetEntity = _weaponEntity; break;
                    case TargetEntity.Tower: targetEntity = _towerEntity; break;
                    case TargetEntity.Barrel: targetEntity = _barrelEntity; break;
                default: targetEntity = _tankEntity; break;
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
            _transformUpdateTags.Set(_tankEntity);
        }

        [Button(nameof(ResetTank)), EnableInPlayMode]
        private void ResetTank()
        {
            _disposeTags.Set(_tankEntity);
            Start();
        }
    }
}
