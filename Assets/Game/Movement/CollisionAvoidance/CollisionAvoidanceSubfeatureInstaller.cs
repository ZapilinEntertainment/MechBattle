using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Movement.CollisionAvoidance
{
    public static class CollisionAvoidanceSubfeatureInstaller
    {
        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<IMovementCellsMap, MovementCellsMap>(Lifetime.Singleton);
            builder.Register<IMovementDensityMap, MovementDensityMap>(Lifetime.Singleton);
            builder.Register<CollisionAvoidanceHandler>(Lifetime.Singleton);
        }
    
        public static void InstallSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<MovementVectorsMapUpdateSystem>(SystemGroupOrder.UnitsNextPositionCalculation);
            installer.AddSystem<MovementDensityMapUpdateSystem>(SystemGroupOrder.UnitsNextPositionCalculation);
            installer.AddSystem<MovementCollisionAvoidanceSystem>(SystemGroupOrder.UnitsNextPositionCalculation);
        }
    }
}
