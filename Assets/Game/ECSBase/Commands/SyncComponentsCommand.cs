using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public static class SyncComponentsCommand
    {
        public static void Execute<T>(Entity receivingEntity, Entity componentOwnerEntity, Stash<T> stash) where T : struct, IComponent
        {
            var originalComponent = stash.Get(componentOwnerEntity, out var exists);
            if (!exists)
            {
                stash.Remove(receivingEntity);
                return;
            }

            stash.Set(receivingEntity, originalComponent);
        }
    
    }
}
