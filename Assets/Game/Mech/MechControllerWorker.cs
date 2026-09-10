using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;
using ZE.Workers;
using VContainer;

namespace ZE.MechBattle
{
    public class MechControllerWorker : Worker
    {
        private Entity _mechEntity;
        private Entity _upperPartEntity;
        private Entity _headEntity;

        private Stash<MechInputComponent> _input;
        private Stash<RotationSpeedComponent> _rotationSpeed;
        private Stash<WeaponTargetPositionComponent> _weaponTargetPositions;
        private Stash<WeaponFireTag> _fireTags;
        private Stash<MechWeaponsComponent> _mechWeapons;
        
        private readonly World _world;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechWeaponsHandler _mechWeaponsHandler;

        [Inject]
        public MechControllerWorker(
            World world, 
            TransformAspectHandler transformAspectHandler, 
            MechWeaponsHandler mechWeaponsHandler)
        {
            _world = world;
            _transformAspectHandler = transformAspectHandler;
            _mechWeaponsHandler = mechWeaponsHandler;

            _input = _world.GetStash<MechInputComponent>();
            _mechWeapons = _world.GetStash<MechWeaponsComponent>();
            _weaponTargetPositions = _world.GetStash<WeaponTargetPositionComponent>();
            _fireTags = _world.GetStash<WeaponFireTag>();
            _rotationSpeed = _world.GetStash<RotationSpeedComponent>();
        }

        public void Start(Entity mechEntity)
        {
            base.Start();

            _mechEntity = mechEntity;
            var mechComponent = _world.GetStash<MechComponent>().Get(_mechEntity);
            _upperPartEntity = mechComponent.UpperPartEntity;
            _headEntity = mechComponent.HeadEntity;
        }

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
            _weaponTargetPositions.Set(_headEntity, new() { Value = pos});
        }

        public void FireMainLeftWeapon()
        {
            var weapons = _mechWeapons.Get(_mechEntity);
            TryFireWeapon(weapons.MainWeaponLeft);
        }

        public void FireMainRightWeapon()
        {
            var weapons = _mechWeapons.Get(_mechEntity);
            TryFireWeapon(weapons.MainWeaponRight);
        }



        public void SwitchEyeFiring(bool active)
        {
            
            if (active)
            {
                _fireTags.Set(_headEntity);
            }
            else
            {
                _fireTags.Remove(_headEntity);
            }
        }

        private void TryFireWeapon(Entity weaponEntity)
        {
            if (_world.IsDisposed(weaponEntity) || !_mechWeaponsHandler.CanWeaponFire(_mechEntity, weaponEntity))
                return;

            _mechWeaponsHandler.FireWeapon(_mechEntity, weaponEntity);
            _fireTags.Set(weaponEntity);
        }
    }
}
