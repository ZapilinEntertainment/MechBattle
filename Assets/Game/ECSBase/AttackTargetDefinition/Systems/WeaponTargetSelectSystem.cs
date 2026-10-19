using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponTargetSelectSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _weaponsFilter;
        private Stash<ClosestAttackTargetComponent> _closestTargets;
        private Stash<AttackTargetComponent> _definedTargets;
        private Stash<WeaponComponent> _weaponComponents;
        private Stash<WeaponRangeComponent> _weaponRange;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public WeaponTargetSelectSystem(TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _weaponsFilter = World.Filter
                .With<WeaponComponent>()
                .Without<AttackTargetComponent>()
                .Build();

            _closestTargets = World.GetStash<ClosestAttackTargetComponent>();
            _definedTargets = World.GetStash<AttackTargetComponent>();
            _weaponComponents = World.GetStash<WeaponComponent>();
            _weaponRange = World.GetStash<WeaponRangeComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            // todo: add selection delay
            // also need check if closest target significally closer

            foreach (var weaponEntity in _weaponsFilter)
            {
                var controllerEntity = _weaponComponents.Get(weaponEntity).CommandEntity;
                var unitTargetComponent = _definedTargets.Get(controllerEntity, out var unitTargetExists);
                var closestTargetComponent = _closestTargets.Get(controllerEntity, out var closestTargetExists);

                if ((unitTargetExists | closestTargetExists) == false)
                    continue;

                Entity definedTarget;
                if (unitTargetExists & closestTargetExists)
                {
                    definedTarget = SelectBestTarget(unitTargetComponent.Entity, closestTargetComponent.Entity, weaponEntity);
                }
                else
                {
                    definedTarget = unitTargetExists ? unitTargetComponent.Entity : closestTargetComponent.Entity;
                }

                _definedTargets.Set(weaponEntity, new() { Entity = definedTarget });
            }
        }

        public void Dispose() { }

        private Entity SelectBestTarget(Entity definedTarget, Entity closestTarget, Entity weaponEntity)
        {
            if (World.IsDisposed(definedTarget))
                return closestTarget;

            if (World.IsDisposed(closestTarget))
                return closestTarget;

            var rangeSq = _weaponRange.Get(weaponEntity).MaxRangeSq;
            var weaponPoint = _transformAspectHandler.GetPoint(weaponEntity);

            var dirA = _transformAspectHandler.GetPosition(definedTarget) - weaponPoint.pos;
            var distanceSqA = math.lengthsq(dirA);
            if (distanceSqA >= rangeSq)
                return closestTarget;

            var dirB = _transformAspectHandler.GetPosition(closestTarget) - weaponPoint.pos;
            var fwd = math.forward(weaponPoint.rot);

            var dotA = math.dot( math.normalize(dirA), fwd);
            var dotB = math.dot(math.normalize(dirB), fwd);

            if (math.abs(dotA - dotB) < 0.1)
            {
                // select by distance
                return distanceSqA < math.lengthsq(dirB) ? definedTarget : closestTarget;
            }
            else
            {
                return dotA > dotB ? definedTarget : closestTarget;
            }
        }
    }
}