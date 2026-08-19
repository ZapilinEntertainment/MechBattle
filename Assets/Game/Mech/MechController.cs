using System;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class MechController : IDisposable
    {
        private readonly Entity _mechEntity;
        private readonly Entity _upperPartEntity;
        private readonly World _world;
        private readonly TransformAspectHandler _transformAspectHandler;

        private Stash<MechInputComponent> _input;
        private Stash<RotationSpeedComponent> _rotationSpeed;
        private Stash<WeaponTargetPositionComponent> _weaponTargetPositions;
        private Stash<WeaponFireTag> _fireTags;
        private Stash<MechWeaponsComponent> _mechWeapons;

        public MechController(World world, TransformAspectHandler transformAspectHandler, Entity mechEntity)
        {
            _world = world;
            _transformAspectHandler = transformAspectHandler;

            _mechEntity = mechEntity;
            var mechComponents = _world.GetStash<MechComponent>();

            _upperPartEntity = mechComponents.Get(_mechEntity).UpperPartEntity;

            _input = _world.GetStash<MechInputComponent>();
            _mechWeapons = _world.GetStash<MechWeaponsComponent>();
            _weaponTargetPositions = _world.GetStash<WeaponTargetPositionComponent>();
            _fireTags = _world.GetStash<WeaponFireTag>();
            _rotationSpeed = _world.GetStash<RotationSpeedComponent>();
        }

        public void Dispose() { }

        public void SetControls(float speed, float steer) => _input.Set(_mechEntity, new() { SpeedValue = speed, SteerValue = steer });

        public void SetUpperPartRotation(float rotationValue, float deltaTime)
        {
            var rotationSpeed = _rotationSpeed.Get(_upperPartEntity).RadianValue;
            var rotationStep = quaternion.AxisAngle(math.up(), deltaTime * rotationValue * rotationSpeed);
            _transformAspectHandler.RotateLocal(_upperPartEntity, rotationStep);
        }

        public void SetMainWeaponsTarget(float3 pos) =>
            _weaponTargetPositions.Set(_upperPartEntity, new() { Value = pos });

        public void SetEyesTarget(float3 pos)
        {
            var weapons = _mechWeapons.Get(_mechEntity);
            _weaponTargetPositions.Set(weapons.LeftEye, new() { Value = pos });
            _weaponTargetPositions.Set(weapons.RightEye, new() { Value = pos });
        }

        public void FireMainWeapon()
        {
            var weapons = _mechWeapons.Get(_mechEntity);
            if (!_world.IsDisposed(weapons.MainWeaponLeft))
                _fireTags.Set(weapons.MainWeaponLeft);

            if (!_world.IsDisposed(weapons.MainWeaponRight))
                _fireTags.Set(weapons.MainWeaponRight);
        }

        public void SwitchEyeFiring(bool active)
        {
            var weapons = _mechWeapons.Get(_mechEntity);

            void SwitchEye(Entity eyeEntity)
            {
                if (_world.IsDisposed(eyeEntity))
                    return;

                if (active)
                {
                    _fireTags.Set(eyeEntity);                    
                }
                else
                {
                    _fireTags.Remove(eyeEntity);
                }
            }

            SwitchEye(weapons.LeftEye);
            SwitchEye(weapons.RightEye);               
        }
    }
}
