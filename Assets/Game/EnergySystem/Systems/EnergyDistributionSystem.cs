using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnergyDistributionSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<EnergySourceComponent> _energySources;
        private Stash<EnergyChargeSpeedComponent> _chargeSpeeds;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EnergySourceComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _energySources = World.GetStash<EnergySourceComponent>();
            _chargeSpeeds = World.GetStash<EnergyChargeSpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var energySourceEntity = _energySources.Get(entity).SourceEntity;
                SyncComponentsCommand.Execute(entity, energySourceEntity, _chargeSpeeds);
            }
        }

        public void Dispose() { }
    }
}