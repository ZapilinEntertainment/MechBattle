using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class IntervalUpdateSystemBase<T> : PausableSystem where T : struct, IIntervalUpdateComponent
    {
        private Filter _filter;
        private Stash<T> _stash;

        public IntervalUpdateSystemBase(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = PrepareFilter().Build();
            _stash = World.GetStash<T>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {
                ref var component = ref _stash.Get(entity);
                var newValue = component.TimeLeft - deltaTime;
                if (newValue > 0f)
                {
                    component.TimeLeft = newValue;
                }                    
                else
                {
                    component.TimeLeft = component.Interval + newValue;
                    IntervalUpdate(entity);
                }                    
            }
        }

        virtual protected FilterBuilder PrepareFilter() => World.Filter.With<T>().Without<EntityDisposeTag>();

        abstract protected void IntervalUpdate(Entity entity);
    }
}