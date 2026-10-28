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
    public sealed class ClosestAttackTargetDefineSystem : PausableSystem 
    {
        private Filter _searchEntitiesFilter;
        private Filter _cellsFilter;
        private Stash<ClosestAttackTargetComponent> _closestTargets;
        private Stash<NextCloseTargetSearchTimeComponent> _cooldownComponents;

        private readonly INavigationMap _navigationMap;
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
            SceneFlagsManager sceneFlagsManager,
            World world) : base(sceneFlagsManager)
        {
            _navigationMap = map;
            _targetDefineProcess = new(_navigationMap, playersList, movementCellsMap, playerRelations, world);
            _applyFunc = TryApplyTarget;
        }

        public override void OnAwake() 
        {
            _searchEntitiesFilter = World.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()                
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Without<NextCloseTargetSearchTimeComponent>()
                .Build();

            _cellsFilter = World.Filter.With<CellMovementDataComponent>().Without<EntityDisposeTag>().Build();

            _closestTargets = World.GetStash<ClosestAttackTargetComponent>();
            _cooldownComponents = World.GetStash<NextCloseTargetSearchTimeComponent>();
        }

        public override void OnUpdate(float deltaTime) 
        {
            if (!_navigationMap.IsInitialized || IsPaused)
                return;

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

            if (_searchEntitiesFilter.IsNotEmpty() && _cellsFilter.IsNotEmpty())
            {
                // preparing job handle (1/2)
                World.JobHandle = _targetDefineProcess.Launch(_searchEntitiesFilter, MAX_HANDLING_ENTITIES_COUNT).prepareJobHandle;                
            }                
        }

        protected override void InternalDispose()
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