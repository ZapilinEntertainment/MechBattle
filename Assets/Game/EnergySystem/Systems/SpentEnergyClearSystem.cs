using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SpentEnergyClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<EnergySpentComponent> _energySpentComponents;

        public void OnAwake() 
        {
            _filter = World.Filter.With<EnergySpentComponent>().Build();
            _energySpentComponents = World.GetStash<EnergySpentComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
                _energySpentComponents.RemoveAll();
        }

        public void Dispose() { }
    }
}