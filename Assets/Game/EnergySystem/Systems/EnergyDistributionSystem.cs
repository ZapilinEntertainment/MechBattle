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
        private Stash<EnergyElementComponent> _elements;
        private Stash<EnergyChargeSpeedComponent> _chargeSpeeds;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EnergyElementComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _elements = World.GetStash<EnergyElementComponent>();
            _chargeSpeeds = World.GetStash<EnergyChargeSpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var energySourceEntity = _elements.Get(entity).EnergySourceEntity;
                SyncComponentsCommand.Execute(entity, energySourceEntity, _chargeSpeeds);
            }
        }

        public void Dispose() { }
    }
}