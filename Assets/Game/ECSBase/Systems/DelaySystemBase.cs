using UnityEngine;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class DelaySystemBase<T> : ISystem where T : struct, IDelayComponent
    {
        public World World { get; set; }
        private Filter _filter;
        protected Stash<T> Stash { get; private set; }

        public virtual void OnAwake()
        {
            _filter = World.Filter.With<T>().Build();
            Stash = World.GetStash<T>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_filter.IsEmpty())
                return;

            var time = Time.time;
            foreach (var entity in _filter)
            {
                if (time > Stash.Get(entity).StopTime)
                {
                    OnDelayCompleted(entity);
                }
            }
        }

        public void Dispose() { }

        protected abstract void OnDelayCompleted(Entity entity);
    }
}