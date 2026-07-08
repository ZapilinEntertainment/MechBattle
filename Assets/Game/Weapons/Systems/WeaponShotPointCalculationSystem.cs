using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponShotPointCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<WeaponShotPoint> _shotPoints;
        private Stash<WeaponTowerComponent> _towerComponents;
        private Stash<WeaponBarrelComponent> _barrelComponents;
        private readonly TransformAspectHandler _transformHandler;

        [Inject]
        public WeaponShotPointCalculationSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<WeaponFireTag>().With<WeaponShotPoint>().Build();

            _shotPoints = World.GetStash<WeaponShotPoint>();
            _towerComponents = World.GetStash<WeaponTowerComponent>();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var weaponEntity in _filter)
            {
                _shotPoints.Get(weaponEntity).WorldPoint = CalculateProjectileLaunchPoint(weaponEntity);
            }
        }

        public void Dispose() { }

        private RigidTransform CalculateProjectileLaunchPoint(Entity weaponEntity)
        {
            var weaponGlobalPoint = _transformHandler.GetPoint(weaponEntity);

            var towerComponent = _towerComponents.Get(weaponEntity, out var towerExists);
            var barrelComponent = _barrelComponents.Get(weaponEntity, out var barrelExists);

            var weaponUp = math.mul(weaponGlobalPoint.rot, math.up());
            var weaponRight = math.mul(weaponGlobalPoint.rot, math.right());

            quaternion towerLocalRot;
            if (towerExists && towerComponent.RotationRadianValue != 0f)
            {
                towerLocalRot = quaternion.AxisAngle(weaponUp, towerComponent.RotationRadianValue);
                weaponRight = math.mul(towerLocalRot, weaponRight);
            }
            else
            {
                towerLocalRot = quaternion.identity;
            }

            quaternion barrelLocalRot;
            if (barrelExists)
            {
                barrelLocalRot = quaternion.AxisAngle(weaponRight, barrelComponent.RadianRotation.x);

                if (barrelComponent.YRotationPossible && barrelComponent.RadianRotation.y != 0f)
                {
                    var yRotation = quaternion.AxisAngle(weaponUp, barrelComponent.RadianRotation.y);
                    barrelLocalRot = math.mul(barrelLocalRot, yRotation);
                }
            }
            else
            {
                barrelLocalRot = quaternion.identity;
            }

            var towerGlobalRot = math.mul(weaponGlobalPoint.rot, towerLocalRot);
            var barrelGlobalRot = math.mul(towerGlobalRot, barrelLocalRot);

            var shotPosition = math.mul(barrelGlobalRot, _shotPoints.Get(weaponEntity).LocalPos);

            return new(rotation: barrelGlobalRot, shotPosition + weaponGlobalPoint.pos);

        }
    }
}