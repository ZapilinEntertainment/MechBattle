using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public static class MovementSystemsInstaller
    {
        public static void RegisterSystems(IContainerBuilder builder)
        {
            builder.Register<HexPathUpdateSystem>(Lifetime.Transient);
            builder.Register<HexPathCalculationSystem>(Lifetime.Transient);

            builder.Register<NavigationPathsList>(_ => new(), Lifetime.Transient);

            builder.Register<NavigationMapInitializer>(Lifetime.Transient);
        }

        public static void Install(SystemsResolver resolver)
        {
            // 1. sets or updates hex-to-hex path (upper nav level)
            resolver.AddSystem<HexPathUpdateSystem>(SystemGroupOrder.RegularUpdate);
            // 2 
            // 3
            // 4 calculte paths (move to different group!)
            resolver.AddSystem<HexPathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            // 5 remove nav components on objects with no move target (use clear group!)

            resolver.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Initialization);
        }

    }
}
