using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class IntervalUpdateSystemBase<T> : PausableSystem where T : struct, IIntervalUpdateComponent
    {
        private Filter _filter;
        protected Stash<T> Stash;

        public IntervalUpdateSystemBase(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = PrepareFilter().Build();
            Stash = World.GetStash<T>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {
                ref var component = ref Stash.Get(entity);
                var newValue = component.TimeLeft - deltaTime;
                if (newValue > 0f)
                {
                    component.TimeLeft = newValue;                    
                }                    
                else
                {
                    RestartTimer(entity, newValue);
                    IntervalUpdate(entity);
                }                    
            }
        }

        virtual protected FilterBuilder PrepareFilter() => World.Filter.With<T>().Without<EntityDisposeTag>();

        abstract protected void IntervalUpdate(Entity entity);

        protected void RestartTimer(Entity entity, float delta = 0f)
        {
            ref var component = ref Stash.Get(entity);
            component.TimeLeft = component.Interval + delta;
        }
    }
}