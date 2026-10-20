using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Movement.CollisionAvoidance
{
    public static class CollisionAvoidanceSubfeatureInstaller
    {
        public class CellDensityApplier : INavigationCellCreationApplier
        {
            private readonly Stash<CellMovementDensityComponent> _movementDensity;

            [Inject]
            public CellDensityApplier(World world)
            {
                _movementDensity = world.GetStash<CellMovementDensityComponent>();
            }

            public void ApplyOnCreate(Entity cellEntity)
            {
                _movementDensity.Add(cellEntity);
            }
        }

        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<IMovementCellsMap, MovementCellsMap>(Lifetime.Singleton);
            builder.Register<IMovementDensityMap, MovementDensityMap>(Lifetime.Singleton);
            builder.Register<CollisionAvoidanceHandler>(Lifetime.Singleton);

            builder.Register<INavigationCellCreationApplier, CellDensityApplier>(Lifetime.Scoped);
        }
    
        public static void InstallSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<MovementVectorsMapUpdateSystem>(SystemGroupOrder.UnitsNextPositionCalculation1);
            installer.AddSystem<MovementDensityMapUpdateSystem>(SystemGroupOrder.UnitsNextPositionCalculation1);
            // ---requires job completion---
            installer.AddSystem<MovementCollisionAvoidanceSystem>(SystemGroupOrder.UnitsNextPositionCalculation3);
        }
    }
}
