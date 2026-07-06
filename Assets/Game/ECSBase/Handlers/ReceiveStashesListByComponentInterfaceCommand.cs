using System;
using System.Linq;
using System.Collections.Generic;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public static class ReceiveStashesListByComponentInterfaceCommand
    {
        public static List<IStash> Execute<T>(World world) where T : IComponent
        {
            var interfaceType = typeof(T);
            var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType &&
                            interfaceType.IsAssignableFrom(t))
                .ToList();

            var list = new List<IStash>();
            foreach (var type in componentTypes)
            {
                list.Add(world.GetReflectionStash(type));
            }
            return list;
        }
    
    }
}
