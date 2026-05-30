using ZE.MechBattle.Navigation;
using Scellecs.Morpeh;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    public interface IEntityPathValidator<PathType> where PathType : ILRUBufferElement
    {
        bool ValidateAndGetCalculationStatus(Entity entity, out PathCalculationStatus status, out PathType path);
    }

    public interface IPathStorage<PathType>
    {
        int Count { get; }
        bool TryGetPathById(int pathId, out PathType path);
        void Remove(int key);
    }

    public class EntityPathValidator<PathType, PathComponent, PathClearTag> : IEntityPathValidator<PathType>
        where PathComponent : struct, IPathUserComponent<int>
        where PathClearTag : struct, IComponent
        where PathType : ILRUBufferElement
    {
        private readonly Stash<PathComponent> _pathComponents;
        private readonly Stash<PathClearTag> _clearTags;
        private readonly LRUDictionaryCache<int, PathCalculationStatus> _pathStatuses;
        private readonly IPathStorage<PathType> _pathsList;

        public EntityPathValidator(
            World world, 
            LRUDictionaryCache<int, PathCalculationStatus> statuses,
            IPathStorage<PathType> pathsList)
        {
            _pathComponents = world.GetStash<PathComponent>();
            _pathStatuses = statuses;
            _pathsList = pathsList;
        }

        public bool ValidateAndGetCalculationStatus(Entity entity, out PathCalculationStatus status, out PathType path)
        {
            var pathId = _pathComponents.Get(entity).PathKey;
            status = _pathStatuses.TryGetCachedValue(pathId, out status) ? status : PathCalculationStatus.Undefined;
            if (!_pathsList.TryGetPathById(pathId, out path))
            {
                _clearTags.Add(entity);
                return false;
            }
            return true;
        }
    }
}
