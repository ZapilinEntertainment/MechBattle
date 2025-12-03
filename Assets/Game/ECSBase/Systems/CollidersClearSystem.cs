using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CollidersClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly CollidersTable _collidersTable;

        [Inject]
        public CollidersClearSystem(CollidersTable collidersTable)
        {
            _collidersTable = collidersTable;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<RegisteredCollidersOwnerTag>().With<EntityDisposeTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
            {
                foreach (var entity in _filter)
                {
                    _collidersTable.UnregisterAllColliders(entity);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}