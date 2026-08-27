using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AttackTargetValidationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _targetCheckFilter;
        private Stash<AttackTargetComponent> _targetsStash;

        public void OnAwake() 
        {
            _targetCheckFilter = World.Filter.With<AttackTargetComponent>().Build();
            _targetsStash = World.GetStash<AttackTargetComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _targetCheckFilter)
            {
                var target = _targetsStash.Get(entity).Entity;
                if (!World.Has(target))
                {
                    _targetsStash.Remove(entity);
                }                    
            }
        }

        public void Dispose() { }
    }
}