using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class PausableSystem : ISystem 
    {
        public World World { get; set;}
        protected bool IsPaused { get; private set; }
        private IDisposable _pauseSubscription;

        public PausableSystem(SceneFlagsManager flags)
        {
            _pauseSubscription = flags.Subscribe<PauseFlag>(x => IsPaused = x);
        }

        public abstract void OnAwake();

        public abstract void OnUpdate(float deltaTime) ;

        public void Dispose()
        {
            _pauseSubscription.Dispose();
            InternalDispose();
        }

        protected virtual void InternalDispose() { }
    }
}