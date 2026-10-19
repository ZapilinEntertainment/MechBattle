using Scellecs.Morpeh;
using System;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
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
        private Stash<NextCloseTargetSearchTimeComponent> _cooldownComponents;

        private readonly TargetDefineProcess _targetDefineProcess;
        private readonly Predicate<(Entity attacker, Entity target)> _applyFunc;
        private const float CLOSEST_TARGET_SEARCH_COOLDOWN = 3f;
        private const int MAX_HANDLING_ENTITIES_COUNT = 1024;

        [Inject]
        public ClosestAttackTargetDefineSystem(
            IMovementCellsMap movementCellsMap, 
            INavigationMap map, 
            IPlayersList playersList, 
            PlayerRelations playerRelations,
            World world)
        {
            _targetDefineProcess = new(map, playersList, movementCellsMap, playerRelations, world);
            _applyFunc = TryApplyTarget;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Without<NextCloseTargetSearchTimeComponent>()
                .Build();
            // todo: add cd component check

            _closestTargets = World.GetStash<ClosestAttackTargetComponent>();
            _cooldownComponents = World.GetStash<NextCloseTargetSearchTimeComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var commitRequired = false;
            switch (_targetDefineProcess.UpdateStage())
            {
                case CalculationProcessStage.Calculating: return;
                case CalculationProcessStage.Complete:
                    {
                        _targetDefineProcess.ApplyResults(_applyFunc);
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
                World.JobHandle = _targetDefineProcess.Launch(_filter, MAX_HANDLING_ENTITIES_COUNT).prepareJobHandle;                
            }                
        }

        public void Dispose()
        {
            _targetDefineProcess.Dispose();
        }

        private bool TryApplyTarget((Entity attacker, Entity target) result)
        {
            if (World.IsDisposed(result.attacker))
                return false;

            var correctTarget = !World.IsDisposed(result.target);
            if (correctTarget)
                _closestTargets.Set(result.attacker, new() { Entity = result.target });
            _cooldownComponents.Set(result.attacker, new(Time.time + CLOSEST_TARGET_SEARCH_COOLDOWN));

            return correctTarget;
        }
    }
}