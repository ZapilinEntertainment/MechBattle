using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class EntityCreationSystemBase<RequestComponentType, FactoryType> : ISystem 
        where RequestComponentType : struct, IComponent
        where FactoryType : IEntityCreationFactory
    {
        public World World { get; set; }
        protected Stash<RequestComponentType> RequestsStash;
        protected readonly FactoryType Factory;
        private Filter _requestsFilter;
        

        [Inject]
        public EntityCreationSystemBase(FactoryType factory)
        {
            Factory = factory;
        }

        public virtual void OnAwake()
        {
            _requestsFilter = World.Filter.With<RequestComponentType>().Build();
            RequestsStash = World.GetStash<RequestComponentType>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_requestsFilter.IsNotEmpty())
            {
                foreach (var requestEntity in _requestsFilter)
                {
                    if (TryExecuteRequest(requestEntity))
                        World.RemoveEntity(requestEntity);
                }
            }
        }

        public void Dispose() { }

        abstract protected bool TryExecuteRequest(Entity requestEntity);
    }
}