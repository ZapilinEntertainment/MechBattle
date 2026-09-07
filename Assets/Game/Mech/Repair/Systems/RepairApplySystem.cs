using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RepairApplySystem : ISystem, IRepairingEntitiesList
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<RepairProcessComponent> _repairProcesses;
        private Stash<HealthComponent> _health;
        private readonly HashSet<Entity> _repairingEntities = new(capacity: 3);

        public void OnAwake() 
        {
            _filter = World.Filter.With<RepairProcessComponent>().Build();

            _repairProcesses = World.GetStash<RepairProcessComponent>();
            _health = World.GetStash<HealthComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            _repairingEntities.Clear();
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var repairVolume = _repairProcesses.Get(entity).RepairVolume;
                ref var healthComponent = ref _health.Get(entity);
                healthComponent.CurrentValue = math.clamp(healthComponent.CurrentValue + repairVolume, 0f, healthComponent.MaxValue);
                _repairingEntities.Add(entity);
            }

            _repairProcesses.RemoveAll();
        }

        public void Dispose() { }

        public bool Contains(Entity entity) => _repairingEntities.Contains(entity);
    }
}