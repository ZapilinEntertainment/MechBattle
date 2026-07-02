using UnityEngine;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitialDelaySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<InitialDelayComponent> _stash;

        public void OnAwake() 
        {
            _filter = World.Filter.With<InitialDelayComponent>().Build();

            _stash = World.GetStash<InitialDelayComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            var time = Time.time;
            foreach (var entity in _filter)
            {
                if (_stash.Get(entity).StopTime >= time)
                {
                    _stash.Remove(entity);
                }
            }
        }

        public void Dispose() { }
    }
}