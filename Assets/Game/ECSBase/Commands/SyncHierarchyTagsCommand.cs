using System.Collections.Generic;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public static class SyncHierarchyTagsCommand
    {
        private static HashSet<Entity> _entitiesList = new();

        /// <summary>
        /// returns true if any structural change done
        /// </summary>
        public static bool Execute<T>(Filter filter, Stash<ParentEntityComponent> parentsStash, Stash<T> tagStash)
            where T : struct, IComponent
        {
            var structuralChanges = CheckAllHierarchy(filter, parentsStash, tagStash);
            if (structuralChanges)
                _entitiesList.Clear();

            return structuralChanges;
        }

        private static bool CheckAllHierarchy<T>(Filter filter, Stash<ParentEntityComponent> parentsStash, Stash<T> tagStash)
            where T : struct, IComponent
        {
            var dirtyFlag = false;
            int newfoundEntitiesCount;
            do
            {
                newfoundEntitiesCount = 0;
                foreach (var entity in filter)
                {
                    if (_entitiesList.Contains(entity))
                        continue;

                    var parentEntity = parentsStash.Get(entity).Value;
                    // if parent contains T-component or will contain(in list, will get component after world.commit)
                    if (tagStash.Has(parentEntity) || _entitiesList.Contains(parentEntity))
                    {
                        tagStash.Set(entity);
                        _entitiesList.Add(entity);
                        //UnityEngine.Debug.Log($"add entity {entity.Id} to update list");
                        newfoundEntitiesCount++;
                    }
                }
                dirtyFlag |= (newfoundEntitiesCount != 0);
            }
            while (newfoundEntitiesCount != 0);
            _entitiesList.Clear();

            return dirtyFlag;
        }

    }
}
