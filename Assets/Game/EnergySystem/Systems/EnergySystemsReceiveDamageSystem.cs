using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Damage;
using ZE.MechBattle.Energy;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnergySystemsReceiveDamageSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<DamageReceivedComponent> _damageReceived;
        private readonly ReceivedDamageList _receivedDamageList;
        private readonly EnergyDamageApplier _energyDamageApplier;
        

        [Inject]
        public EnergySystemsReceiveDamageSystem(ReceivedDamageList receivedDamageList, EnergyDamageApplier energyDamageApplier)
        {
            _receivedDamageList = receivedDamageList;
            _energyDamageApplier = energyDamageApplier;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<DamageReceivedComponent>()
                .With<EnergyCellsGridComponent>()
                .Build();

            _damageReceived = World.GetStash<DamageReceivedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var receivedDamageVolume = _receivedDamageList[entity];
                var maxDamageProducer = _damageReceived.Get(entity).MaxDamageProducer;
                var excessDamage = _energyDamageApplier.ApplyDamageToEnergyGrid(entity, receivedDamageVolume.Volume, maxDamageProducer);
                if (excessDamage == 0f)
                {
                    _receivedDamageList.RemoveDamage(entity);
                }                    
                else
                {
                    // will be applied to health
                    _receivedDamageList.UpdateDamage(entity, new() { Volume = excessDamage, Flags = ReceivedDamageFlag.ExcessDamageTransfer });
                }
            }
        }

        public void Dispose() { }
    }
}