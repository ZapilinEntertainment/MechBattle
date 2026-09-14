using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public static class TransferComponentsCommand
    {
        public static void Execute<T1, T2, T3>(Entity sourceEntity, Entity receivingEntity, Stash<T1> stash1, Stash<T2> stash2, Stash<T3> stash3, bool removeOriginalComponents = true) 
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var component1 = stash1.Get(sourceEntity, out var componentExists1);
            var component2 = stash2.Get(sourceEntity, out var componentExists2);
            var component3 = stash3.Get(sourceEntity, out var componentExists3);

            if (removeOriginalComponents)
            {
                stash1.Remove(sourceEntity);
                stash2.Remove(sourceEntity);
                stash3.Remove(sourceEntity);
            }


            if (componentExists1)
                stash1.Set(receivingEntity, component1);

            if (componentExists2)
                stash2.Set(receivingEntity, component2);

            if (componentExists3)
                stash3.Set(receivingEntity, component3);
        }
    
    }
}
