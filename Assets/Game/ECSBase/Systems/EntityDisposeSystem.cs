using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EntityDisposeSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly ChildEntitiesUpdateHandler _childEntitiesUpdateHandler;

        [Inject]
        public EntityDisposeSystem(ChildEntitiesUpdateHandler childEntitiesUpdateSystem)
        {
            _childEntitiesUpdateHandler = childEntitiesUpdateSystem;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<EntityDisposeTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            _childEntitiesUpdateHandler.ClearEntitiesWithDisposedParents();


            if (_filter.IsNotEmpty())
            {
                foreach (var entity in _filter)
                    World.RemoveEntity(entity);
            }
        }

        public void Dispose() { }
    }
}