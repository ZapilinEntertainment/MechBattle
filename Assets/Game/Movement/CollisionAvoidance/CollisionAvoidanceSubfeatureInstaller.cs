using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public static class CollisionAvoidanceSubfeatureInstaller
    {
        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<IMovementCellsMap, MovementCellsMap>(Lifetime.Singleton).AsSelf();
            builder.Register<MovementDensityMapsManager>(Lifetime.Singleton);
            builder.Register<CollisionAvoidanceHandler>(Lifetime.Singleton);
        }
    
        public static void InstallSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<MovementVectorsMapUpdateSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<MovementCollisionAvoidanceSystem>(SystemGroupOrder.UnitsNextPositionCalculation);
        }
    }
}
