using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadDecreesHandleSystem : ISystem 
    {
        // why discrete system - may need jobs for mass-calculation

        public World World { get; set;}
        private Filter _handleFilter;
        private Stash<SquadDecreeComponent> _decrees;
        private Stash<UnhandledDecreeTag> _tags;
        private readonly SquadDecreeApplier _decreeApplier;

        [Inject]
        public SquadDecreesHandleSystem(SquadDecreeApplier squadDecreeApplier)
        {
            _decreeApplier = squadDecreeApplier;
        }

        public void OnAwake() 
        {
            _handleFilter = World.Filter
                .With<SquadDecreeComponent>()
                .With<UnhandledDecreeTag>()
                .Without<SquadUpdateRequiredTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _decrees = World.GetStash<SquadDecreeComponent>();
            _tags = World.GetStash<UnhandledDecreeTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_handleFilter.IsEmpty())
                return;

            foreach (var squadEntity in _handleFilter)
            {
                var decreeeType = _decrees.Get(squadEntity).Type;
                // only move decreee realised now
                _decreeApplier.ApplyMovementDecree(squadEntity);
            }
            _tags.RemoveAll();
        }

        public void Dispose() { }
    }
}