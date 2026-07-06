using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AttackTargetValidationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<AttackTargetComponent> _stash;

        public void OnAwake() 
        {
            _filter = World.Filter.With<AttackTargetComponent>().Build();
            _stash = World.GetStash<AttackTargetComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var target = _stash.Get(entity).Entity;
                if (!World.Has(target))
                {
                    _stash.Remove(entity);
                }                    
            }
        }

        public void Dispose() { }
    }
}