using Scellecs.Morpeh;
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

        private class UnitCollisionAvoidanceApplier : IUnitFeatureApplier
        {
            private readonly Stash<MovementCollisionAvoidanceComponent> _movementCollisionAvoidanceComponents;

            public UnitCollisionAvoidanceApplier(World world)
            {
                _movementCollisionAvoidanceComponents = world.GetStash<MovementCollisionAvoidanceComponent>();
            }

            public void ApplyUnitFeature(Entity entity, IUnitConfig config)
            {
                _movementCollisionAvoidanceComponents.Add(entity, new(config.CollisionAvoidancePriority));
            }
        }

        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<IMovementCellsMap, MovementCellsMap>(Lifetime.Singleton);
            builder.Register<IMovementDensityMap, MovementDensityMap>(Lifetime.Singleton);
            builder.Register<CollisionAvoidanceHandler>(Lifetime.Singleton);

            builder.Register<INavigationCellCreationApplier, CellDensityApplier>(Lifetime.Scoped);
            builder.Register<IUnitFeatureApplier, UnitCollisionAvoidanceApplier>(Lifetime.Scoped);
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
