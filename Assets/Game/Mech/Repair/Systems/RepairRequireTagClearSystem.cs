using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RepairRequireTagClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<RepairRequiredTag> _tags;
        private Stash<HealthComponent> _healths;

        public void OnAwake() 
        {
            _filter = World.Filter.With<RepairRequiredTag>().Build();

            _tags = World.GetStash<RepairRequiredTag>();
            _healths = World.GetStash<HealthComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                if (!_healths.Get(entity).IsDamaged)
                    _tags.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}