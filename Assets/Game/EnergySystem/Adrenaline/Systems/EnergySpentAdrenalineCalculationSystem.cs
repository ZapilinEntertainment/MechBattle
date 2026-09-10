using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnergySpentAdrenalineCalculationSystem : ISystem 
    {
        // not implemented
        public World World { get; set;}
        private Filter _filter;
        private Stash<AdrenalineComponent> _adrenaline;
        private Stash<EnergySpentComponent> _energySpent;
        private Stash<EnergySpentAdrenalineComponent> _adrenalineAdditions;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AdrenalineComponent>()
                .With<EnergySpentComponent>()
                .Build();

            _adrenaline = World.GetStash<AdrenalineComponent>();
            _energySpent = World.GetStash<EnergySpentComponent>();
            _adrenalineAdditions = World.GetStash<EnergySpentAdrenalineComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var energy = _energySpent.Get(entity).Volume;
                var adrenaline = _adrenaline.Get(entity);
                var adrenalineVolume = energy * adrenaline.AdrenalineEnergyConsumptionCf;
                _adrenalineAdditions.Set(entity, new() { AdrenalineVolume = adrenalineVolume });
            }
        }

        public void Dispose() { }
    }
}