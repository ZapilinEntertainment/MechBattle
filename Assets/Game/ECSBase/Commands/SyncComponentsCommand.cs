using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public static class SyncComponentsCommand
    {
        public static void Execute<T>(Entity childEntity, Entity parentEntity, Stash<T> stash) where T : struct, IComponent
        {
            var originalComponent = stash.Get(parentEntity, out var exists);
            if (!exists)
                return;
            stash.Set(childEntity, originalComponent);
        }
    
    }
}
