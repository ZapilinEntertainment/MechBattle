using R3;
using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class UIEnergyCellViewWorker : Worker
    {
        private Entity _cellEntity;
        private EnergyCellUiView _view;

        private readonly Stash<EnergyChargeComponent> _chargeComponents;
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly Stash<RepairRequiredTag> _repairRequired;
        private readonly Stash<RepairProcessComponent> _repairProcess;
        private readonly Stash<DamageReceivedComponent> _damageReceived;

        [Inject]
        public UIEnergyCellViewWorker(World world)
        {
            _healthComponents = world.GetStash<HealthComponent>();
            _repairProcess = world.GetStash<RepairProcessComponent>();
            _repairRequired = world.GetStash<RepairRequiredTag>();
            _chargeComponents = world.GetStash<EnergyChargeComponent>();
            _damageReceived = world.GetStash<DamageReceivedComponent>();
        }

        public void Start(Entity cellEntity, EnergyCellUiView view)
        {
            _cellEntity = cellEntity;
            _view = view;
            _view.AddTo(CompositeDisposable);
        }

        public void Update()
        {
            var repairInProgress = _repairProcess.Has(_cellEntity);
            var requireRepairs = _repairRequired.Has(_cellEntity);
            var mainLineColor = requireRepairs ? (repairInProgress ? _view.RepairColor : _view.HealthColor) : _view.EnergyColor;
            float mainLineValue;
            if (requireRepairs)
                mainLineValue = _healthComponents.Get(_cellEntity).HealthPercent;
            else
                mainLineValue = _chargeComponents.Get(_cellEntity).ChargePercent;

            _view.UpdateValues(new()
            {
                EnableDamageLine = requireRepairs,
                DamageLineFillPc = requireRepairs ? math.clamp(1f - mainLineValue, 0f, 1f) : 0f,

                EnableRepairMarker = repairInProgress,
                EnergyLineColor = mainLineColor,
                EnergyLineFillPc = mainLineValue,
                HasReceivedDamageOnThisFrame = _damageReceived.Has(_cellEntity)
            });
        }
    }
}
