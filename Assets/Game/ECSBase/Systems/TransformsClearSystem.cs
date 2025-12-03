using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformsClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<TransformComponent> _transforms;
        private readonly TransformAccessManager _transformsManager;

        [Inject]
        public TransformsClearSystem(TransformAccessManager manager)
        {
            _transformsManager = manager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TransformComponent>()
                .With<EntityDisposeTag>()
                .Build();

            _transforms = World.GetStash<TransformComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                _transformsManager.UnregisterTransform(_transforms.Get(entity).Key);
            }
        }

        public void Dispose() { }
    }
}