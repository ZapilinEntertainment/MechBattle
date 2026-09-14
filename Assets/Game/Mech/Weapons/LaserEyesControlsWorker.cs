using R3;
using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class LaserEyesControlsWorker : Worker
    {
        public Observable<bool> AreLaserEyesActiveProperty => _areLaserEyesActiveProperty;

        private Entity _headEntity;
        private readonly Stash<ChargingWeaponTag> _chargeTags;
        private readonly Stash<WeaponTargetPositionComponent> _weaponTargetPositions;
        private readonly Stash<WeaponFireTag> _fireTags;
        private readonly WeaponHandler _weaponHandler;
        private readonly ReactiveProperty<bool> _areLaserEyesActiveProperty = new(false);

        [Inject]
        public LaserEyesControlsWorker(World world, WeaponHandler weaponHandler)
        {
            _weaponTargetPositions = world.GetStash<WeaponTargetPositionComponent>();
            _chargeTags = world.GetStash<ChargingWeaponTag>();
            _fireTags = world.GetStash<WeaponFireTag>();

            _weaponHandler = weaponHandler;

            Observable.EveryUpdate().Subscribe(Update).AddTo(CompositeDisposable);
        }

        public void Start(Entity headEntity)
        {
            _headEntity = headEntity;
        }

        public void StartEmit()
        {
            if (!_areLaserEyesActiveProperty.Value)
                   _chargeTags.Set(_headEntity);
        }

        public void EndEmit()
        {
            if (_areLaserEyesActiveProperty.Value)
                _weaponHandler.ReleaseChargingWeapon(_headEntity);
        }
    
        public void SetTargetPos(float3 pos)
        {
            _weaponTargetPositions.Set(_headEntity, new() { Value = pos });
        }

        private void Update(Unit unit)
        {
            _areLaserEyesActiveProperty.Value = _fireTags.Has(_headEntity) || _chargeTags.Has(_headEntity);
        }
    }
}
