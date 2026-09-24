using Scellecs.Morpeh;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle
{
    public static class IncreaseComponentValueCommand
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute<ComponentType>(Entity entity, Stash<ComponentType> stash, float value)
            where ComponentType : struct, IValueComponent<float>
        {
            ref var component = ref stash.Get(entity, out var exist);
            if (exist)
                component.Value+=value;
            else
                stash.Set(entity, new() { Value = value });

        }

    }
}
