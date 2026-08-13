using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LifetimeTrackingSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;

        [Inject]
        public LifetimeTrackingSystem(LifetimeTrackingManager lifetimeTrackingManager)
        {
            _lifetimeTrackingManager = lifetimeTrackingManager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EntityDisposeTag>()
                .With<LifetimeTrackingTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _lifetimeTrackingManager.OnEntityDisposed(entity);
            }
        }

        public void Dispose() { }
    }
}