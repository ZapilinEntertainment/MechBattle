using Scellecs.Morpeh;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle
{
    public interface IValueComponent<T> : IComponent
    {
        T Value { get; set; }
    }

    public static class GetComponentOrDefaultValueCommand
        
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueType Execute<ValueType, ComponentType>(Entity entity, Stash<ComponentType> stash, ValueType defaultValue)
            where ComponentType : unmanaged, IValueComponent<ValueType>
        {
            var component = stash.Get(entity, out var exists);
            return exists ? component.Value : defaultValue;
        }
    
    }
}
