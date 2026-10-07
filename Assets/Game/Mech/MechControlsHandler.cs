using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechControlsHandler
    {
        private readonly TransformAspectHandler _transformAspectHandler;

        private readonly Stash<WeaponTargetPositionComponent> _weaponTargetPositions;
        private readonly Stash<MechComponent> _mechComponents;
        private readonly Stash<AttackTargetComponent> _attackTargets;
        private readonly Stash<RotationSpeedComponent> _rotationSpeed;

        [Inject]
        public MechControlsHandler(World world, TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;

            _weaponTargetPositions = world.GetStash<WeaponTargetPositionComponent>();
            _mechComponents = world.GetStash<MechComponent>();
            _attackTargets = world.GetStash<AttackTargetComponent>();
            _rotationSpeed = world.GetStash<RotationSpeedComponent>();
        }

        public void RotateUpperPart(Entity mechEntity, float rotationValue) => RotateUpperPart(mechEntity, GetUpperPartEntity(mechEntity), rotationValue);
        public void RotateUpperPart(Entity mechEntity, Entity upperPartEntity, float rotationValue)
        {
            var rotationSpeed = _rotationSpeed.Get(upperPartEntity).RadianValue;
            var rotationStep = quaternion.AxisAngle(math.up(), rotationValue * rotationSpeed);
            _transformAspectHandler.RotateLocal(upperPartEntity, rotationStep);
        }

        /// <returns> true if default rotation reached</returns>
        public bool NormalizeUpperPartRotation(Entity mechEntity, float dt)
        {
            var upperPartEntity = GetUpperPartEntity(mechEntity);
            var rotationSpeed = _rotationSpeed.Get(upperPartEntity).RadianValue;
            return _transformAspectHandler.RotateLocal(upperPartEntity, quaternion.identity, rotationSpeed * dt);
        }

        // player's method: set target via cursor, not for auto-shot weapons
        public void SetMainWeaponsTarget(Entity mechEntity, Entity upperPartEntity, float3 pos) =>
           _weaponTargetPositions.Set(upperPartEntity, new() { Value = pos });


        // for auto shot weapons: target position will be calculated in WeaponTargetPositionSetSystem
        public void SetMainWeaponsTarget(Entity mechEntity, Entity upperPartEntity, Entity targetEntity) =>
            _attackTargets.Set(upperPartEntity, new(targetEntity));

        public Entity GetUpperPartEntity(Entity mechEntity) => _mechComponents.Get(mechEntity).UpperPartEntity;

    }
}
