using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PartitionsClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly PartitionsList _partitionsList;

        [Inject]
        public PartitionsClearSystem(PartitionsList partitionsList)
        {
            _partitionsList = partitionsList;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<PartitionsRootTag>().With<EntityDisposeTag>().Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _partitionsList.OnRootEntityDisposed(entity);
            }
        }

        public void Dispose() { }
    }
}