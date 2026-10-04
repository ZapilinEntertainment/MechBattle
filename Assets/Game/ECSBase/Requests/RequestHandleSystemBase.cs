using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class RequestHandleSystemBase<T> : ISystem 
        where T : struct, IRequestComponent
    {
        public World World { get; set;}

        protected Filter _requestsFilter;
        private Stash<T> _requests;
        private Stash<CompletedRequestTag> _completedTags;        

        public virtual void OnAwake()
        {
            _requestsFilter = World.Filter.With<T>().Build();

            _requests = World.GetStash<T>();
            _completedTags = World.GetStash<CompletedRequestTag>();
        }

        public virtual void OnUpdate(float deltaTime)
        {
            foreach (var requestEntity in _requestsFilter)
            {
                if (TryHandleRequest(requestEntity))
                    OnRequestHandled(requestEntity);
                else
                    OnRequestDenied(requestEntity);
            }
        }

        public virtual void Dispose() { }

        protected abstract bool TryHandleRequest(Entity requestEntity);

        protected virtual void OnRequestHandled(Entity entity) => _completedTags.Add(entity);
        protected virtual void OnRequestDenied(Entity entity) => World.RemoveEntity(entity);
        protected T GetRequestComponent(Entity entity) => _requests.Get(entity);
    }
}