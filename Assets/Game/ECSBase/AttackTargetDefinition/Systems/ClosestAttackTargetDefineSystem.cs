using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ClosestAttackTargetDefineSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ClosestAttackTargetComponent> _closestTargets;

        private readonly TargetDefineProcess _targetDefineProcess;
        private readonly Action<Entity, Entity> _applyAction;

        [Inject]
        public ClosestAttackTargetDefineSystem(
            IMovementCellsMap movementCellsMap, 
            INavigationMap map, 
            IPlayersList playersList, 
            PlayerRelations playerRelations,
            World world)
        {
            _targetDefineProcess = new(map, playersList, movementCellsMap, playerRelations, world);
            _applyAction = ApplyTarget;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Build();
            // todo: add cd component check

            _closestTargets = World.GetStash<ClosestAttackTargetComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            // TODO: add search cd (for situations with no enemies in radius)
            var commitRequired = false;
            switch (_targetDefineProcess.Stage)
            {
                case CalculationProcessStage.Calculating: return;
                case CalculationProcessStage.Complete:
                    {
                        _targetDefineProcess.ApplyResults(_applyAction);
                        commitRequired = true;
                        break;
                    }
            }

            // before launch job
            if (commitRequired)
                World.Commit();

            if (_filter.IsNotEmpty())
            {
                // preparing job handle (1/2)
                World.JobHandle = _targetDefineProcess.Launch(_filter).prepareJobHandle;                
            }                
        }

        public void Dispose()
        {
            _targetDefineProcess.Dispose();
        }

        private void ApplyTarget(Entity attacker, Entity target)
        {
            _closestTargets.Set(attacker, new() { Entity = target });
            // todo: add cd component
        }
    }
}