using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using Unity.Mathematics;
using ZE.Workers;
using VContainer;
using R3;

namespace ZE.MechBattle
{
    public class MechControlsWorker : Worker
    {
        public Observable<bool> LaserEyesActiveProperty => _laserEyesWorker.AreLaserEyesActiveProperty;

        private Entity _mechEntity;
        private Entity _upperPartEntity;

        private Stash<MechInputComponent> _input;
        private Stash<RotationSpeedComponent> _rotationSpeed;
        private Stash<WeaponTargetPositionComponent> _weaponTargetPositions;
        private Stash<WeaponFireTag> _fireTags;
        private LaserEyesControlsWorker _laserEyesWorker;
        
        private readonly World _world;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly MechWeaponsHandler _mechWeaponsHandler;
        private readonly IWeaponsManager _weaponsManager;

        private readonly MechWeaponKey PRIMARY_WEAPON_KEY_LEFT = new(MechWeaponGroup.Primary, 0);
        private readonly MechWeaponKey PRIMARY_WEAPON_KEY_RIGHT = new(MechWeaponGroup.Primary, 1);

        [Inject]
        public MechControlsWorker(
            World world, 
            TransformAspectHandler transformAspectHandler, 
            MechWeaponsHandler mechWeaponsHandler,
            IWeaponsManager weaponsManager)
        {
            _world = world;
            _transformAspectHandler = transformAspectHandler;
            _mechWeaponsHandler = mechWeaponsHandler;
            _weaponsManager = weaponsManager;

            _input = _world.GetStash<MechInputComponent>();
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

            _laserEyesWorker = AddSubWorker<LaserEyesControlsWorker>();
            _laserEyesWorker.Start(mechComponent.HeadEntity);
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

        public void SetEyesTarget(float3 pos) => _laserEyesWorker.SetTargetPos(pos);

        public void FireMainLeftWeapon()
        {
            if (_weaponsManager.TryGetWeapon(_mechEntity, PRIMARY_WEAPON_KEY_LEFT, out var primaryWeaponLeft))
                TryFireWeapon(primaryWeaponLeft);
        }

        public void FireMainRightWeapon()
        {
            if (_weaponsManager.TryGetWeapon(_mechEntity, PRIMARY_WEAPON_KEY_RIGHT, out var primaryWeaponRight))
                TryFireWeapon(primaryWeaponRight);
        }



        public void SwitchEyeFiring(bool active) 
        {            
            if (active)
            {
                _laserEyesWorker.StartEmit();                
            }
            else
            {                
                _laserEyesWorker.EndEmit();
            }
        }

        private void TryFireWeapon(Entity weaponEntity)
        {
            if (_world.IsDisposed(weaponEntity) || !_mechWeaponsHandler.CanWeaponFire(_mechEntity, weaponEntity))
                return;

            _mechWeaponsHandler.TrySpendEnergyForWeaponShot(_mechEntity, weaponEntity, out var shortage);
            // no sactions needed: shot possibility checks before shooting

            _fireTags.Set(weaponEntity);
        }
    }
}
