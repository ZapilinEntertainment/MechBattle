using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GravityAccelerationIncreaseSystem : PausableSystem 
    {
        private Filter _filter;
        private Stash<GravityAccelerationValueComponent> _gravityAcceleration;

        public GravityAccelerationIncreaseSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<GravityAccelerationValueComponent>().Build();
            _gravityAcceleration = World.GetStash<GravityAccelerationValueComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _filter.IsEmpty())
                return;

            var step = Physics.gravity.y * deltaTime;
            // note: value is negative
            foreach (var entity in _filter)
            {
                ref var component = ref _gravityAcceleration.Get(entity);
                component.CurrentValue -= step * component.AffectionCf;
            }
        }
    }
}