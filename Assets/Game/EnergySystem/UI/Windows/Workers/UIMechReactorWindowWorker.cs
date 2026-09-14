using Scellecs.Morpeh;
using ZE.Workers;
using R3;
using ZE.MechBattle.Ecs;
using VContainer;

namespace ZE.MechBattle
{
    public class UIMechReactorWindowWorker : Worker
    {
        private Entity _mechEntity;
        private Entity _reactorEntity;

        private readonly ReactiveProperty<UIMechReactorWindow.UpdateProtocol> _protocolReactiveProperty = new();
        private readonly World _world;

        private readonly Stash<AdrenalineComponent> _adrenaline;
        private readonly Stash<HealthComponent> _health;
        private readonly Stash<EnergyChargeSpeedComponent> _energyChargeSpeed;
        private readonly Stash<RepairSpeedComponent> _repairSpeed;

        [Inject]
        public UIMechReactorWindowWorker(World world)
        {
            _world = world;

            _adrenaline = _world.GetStash<AdrenalineComponent>();
            _health = _world.GetStash<HealthComponent>();
            _energyChargeSpeed = _world.GetStash<EnergyChargeSpeedComponent>();
            _repairSpeed = _world.GetStash<RepairSpeedComponent>();
        }
    
        public void Start(Entity mechEntity, UIMechReactorWindow window)
        {
            _mechEntity = mechEntity;

            var energySources = _world.GetStash<EnergySourceComponent>();
            _reactorEntity = energySources.Get(_mechEntity).SourceEntity;

            var adrenaline = _adrenaline.Get(_mechEntity);
            window.SetupAdrenalineSteps(adrenaline.MaxAdrenalineLevel);

            UpdateData(Unit.Default);
            _protocolReactiveProperty.Subscribe(window.UpdateData).AddTo(CompositeDisposable);
            Observable.EveryUpdate().Subscribe(UpdateData).AddTo(CompositeDisposable);
        }

        public override void Dispose()
        {
            base.Dispose();
            _protocolReactiveProperty.Dispose();
        }

        private void UpdateData(Unit unit)
        {
            if (_world.IsDisposed)
                return;

            var adrenalineComponent = _adrenaline.Get(_mechEntity);

            _protocolReactiveProperty.Value = new()
            {
                AdrenalineLevelPc = adrenalineComponent.AdrenalineLevelPc,
                ReactorConditionPc = _health.Get(_reactorEntity).HealthPercent,

                EnergySurplus = (int)_energyChargeSpeed.Get(_reactorEntity).Value,
                EnergyBoostSignValue = ParameterCfToSign(adrenalineComponent.EnergyProductionCf),

                RepairSpeed = (int)_repairSpeed.Get(_mechEntity).Value,
                RepairBoostSignValue = ParameterCfToSign(adrenalineComponent.RepairSpeedCf)
            };
        }

        private int ParameterCfToSign(float cf) => cf == 1f ? 0 : (cf > 1f ? 1 : -1);
    }
}
