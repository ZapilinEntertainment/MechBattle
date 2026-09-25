using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using System.Runtime.CompilerServices;
using Unity.Burst;

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

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute<T>(Entity receivingEntity, Entity componentOwnerEntity, NativeStash<T> stash) where T : unmanaged, IComponent
        {
            var originalComponent = stash.Get(componentOwnerEntity, out var exists);
            stash.Get(receivingEntity) = originalComponent;
        }
    }
}
