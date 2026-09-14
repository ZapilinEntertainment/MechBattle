using R3;
using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class UILaserEyesPanelWorker : Worker
    {
        private bool _isDisplaying = false;
        private UILaserEyesWindow _window;
        private Entity _weaponEntity;

        private readonly World _world;
        private readonly IMechUIElementsVisibilityController _visibilityController;
        private readonly Stash<ChargingWeaponTag> _chargeTags;
        private readonly Stash<WeaponFireTag> _fireTags;
        private readonly Stash<WeaponChargeComponent> _chargeComponents;

        [Inject]
        public UILaserEyesPanelWorker(IMechUIElementsVisibilityController visibilityController, World world)
        {
            _visibilityController = visibilityController;
            _world = world;

            _chargeTags = world.GetStash<ChargingWeaponTag>();
            _fireTags = world.GetStash<WeaponFireTag>();
            _chargeComponents = world.GetStash<WeaponChargeComponent>();
        }

        public void Start(Entity weaponEntity, UILaserEyesWindow laserEyesWindow)
        {
            _weaponEntity = weaponEntity;
            _window = laserEyesWindow;

            _visibilityController
                .VisibilityFlagsProperty
                .Select(flags => flags.HasFlag(MechInterfaceElementsVisibilityFlags.LaserEyesAim))
                .Subscribe(x =>
                {
                    _isDisplaying = x;
                    _window.SetVisibility(x);
                })
                .AddTo(CompositeDisposable);

            Observable
                .EveryUpdate()
                .Where(_ => _isDisplaying)
                .Subscribe(Update)
                .AddTo(CompositeDisposable);
        }

        private void Update(Unit unit)
        {
            if (_world.IsDisposed)
                return;
            // todo: add Localization

            var chargeComponent = _chargeComponents.Get(_weaponEntity);

            var status = WeaponChargeStatus.Idle;
            var text = string.Empty;
            var overchargePercent = chargeComponent.OverchargePercent;
            if (_chargeTags.Has(_weaponEntity))
            {
                if (overchargePercent != 0f)
                {
                    status = WeaponChargeStatus.ReadyToDischarge;
                    text = "Release to discharge";
                }
                else
                {
                    status = WeaponChargeStatus.Charge;
                    text = "Charging...";
                }                    
            }
            else
            {
                if (_fireTags.Has(_weaponEntity)) 
                {
                    status = WeaponChargeStatus.Discharging;
                    text = "Discharging!";
                }
            }

            _window.UpdateData(new()
            {
                ChargePercent = chargeComponent.ChargePercent,
                OverchargePercent = overchargePercent,
                Status = status,
                LabelText = text
            });
        }
        
    }
}
