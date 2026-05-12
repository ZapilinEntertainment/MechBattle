using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class PathsAccountingSystemBase<UserKey> : ISystem where UserKey : IUserCountDependentLRUPathsBuffer<Entity>
    {
        public World World { get; set; }

        abstract protected int BufferLimit { get; }
        abstract protected Filter ActivePathUsersFilter { get; }

        private HashSet<Entity> _activePathUsers = new();
        private List<Entity> _clearUsersList = new();
        private readonly IUserCountDependentLRUPathsBuffer<Entity> _buffer;
        private readonly IBufferTrimController _bufferClearController;



        [Inject]
        public PathsAccountingSystemBase(IUserCountDependentLRUPathsBuffer<Entity> buffer)
        {
            _buffer = buffer;
            _bufferClearController = _buffer.CreateTrimController();
        }

        public void OnUpdate(float deltaTime)
        {
            _activePathUsers.Clear();
            _clearUsersList.Clear();

            // users with no valid paths
            foreach (var u2pKvp in _buffer.UserToPathId)
            {
                var entity = u2pKvp.Key;
                if (HasPathComponent(entity))
                    _activePathUsers.Add(entity);
                else 
                    _clearUsersList.Add(entity);                    
            }

            foreach (var entity in _clearUsersList)
            {
                _buffer.OnPathUserLeft(entity);
            }

            // users that just started use path
            foreach (var entity in _activePathUsers)
            {
                if (_activePathUsers.Add(entity))
                {
                    _buffer.OnPathStartUse(entity, GetPathId(entity));
                }
            }

            // clear buffer of obsolete paths
            var pathsCount = _buffer.PathsCount;
            if (pathsCount > BufferLimit)
                _bufferClearController.Trim(BufferLimit);
        }

        public void Dispose() { }

        abstract public void OnAwake();

        abstract protected bool HasPathComponent(Entity entity);
        abstract protected int GetPathId(Entity entity);
    }
}