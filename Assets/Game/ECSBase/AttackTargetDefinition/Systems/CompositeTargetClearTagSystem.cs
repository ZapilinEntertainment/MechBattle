using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CompositeTargetClearTagSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;
        private Stash<CompositeTargetSpecifiedTag> _compositeTargetSpecifiedTags;

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<CompositeTargetSpecifiedTag>()
                .Without<AttackTargetComponent>()
                .Build();

            _compositeTargetSpecifiedTags = World.GetStash<CompositeTargetSpecifiedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                _compositeTargetSpecifiedTags.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}