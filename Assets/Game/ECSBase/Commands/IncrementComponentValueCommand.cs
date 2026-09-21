using Scellecs.Morpeh;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle
{
    public static class IncrementComponentValueCommand
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute<ComponentType>(Entity entity, Stash<ComponentType> stash)
            where ComponentType : struct, IValueComponent<int>
        {
            ref var component = ref stash.Get(entity, out var exist);
            if (exist)
                component.Value++;
            else
                stash.Set(entity, new() { Value = 1 });

        }
    
    }
}
