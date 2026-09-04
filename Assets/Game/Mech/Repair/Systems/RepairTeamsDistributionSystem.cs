using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RepairTeamsDistributionSystem : ISystem
    {
        private readonly struct RepairCandidate : IComparable<RepairCandidate>
        {
            public readonly Entity RepairableEntity;
            public readonly Entity RepairProduceEntity;
            public readonly RepairPriority Priority;

            public RepairCandidate(Entity repairableEntity, Entity repairProducingEntity, RepairPriority repairPriority)
            {
                RepairableEntity = repairableEntity;
                RepairProduceEntity = repairProducingEntity;
                Priority = repairPriority;
            }

            public int CompareTo(RepairCandidate other) => Priority.CompareTo(other.Priority);
        }

        public World World { get; set;}
        private Filter _repairProduceEntitiesFilter;
        private Filter _repairTargetsFilter;
        private Stash<RepairableComponent> _repairables;
        private Stash<RepairTeamsComponent> _repairTeams;
        private Stash<RepairSpeedComponent> _repairSpeed;
        private Stash<RepairProcessComponent> _repairProcesses;

        private readonly List<RepairCandidate> _candidatesList = new();

        public void OnAwake() 
        {
            _repairProduceEntitiesFilter = World.Filter
                .With<RepairTeamsComponent>()
                .Build();

            _repairTargetsFilter = World.Filter
                .With<RepairRequiredTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _repairables = World.GetStash<RepairableComponent>();
            _repairTeams = World.GetStash<RepairTeamsComponent>();
            _repairSpeed = World.GetStash<RepairSpeedComponent>();
            _repairProcesses = World.GetStash<RepairProcessComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            // restore all teams count before distribution
            foreach (var repairProduceEntity in _repairProduceEntitiesFilter)
            {
                ref var teamsComponent = ref _repairTeams.Get(repairProduceEntity);
                teamsComponent.FreeTeamsCount = teamsComponent.TotalTeamsCount;
            }



            if (_repairTargetsFilter.IsEmpty())
                return;

            foreach (var entity in _repairTargetsFilter)
            {
                var repairableComponent = _repairables.Get(entity);
                _candidatesList.Add(new(entity, repairableComponent.RepairProducingEntity, repairableComponent.Priority));
            }

            var count = _candidatesList.Count;
            if (count == 0)
                return;


            _candidatesList.Sort();

            for (var i = 0; i < count; i++)
            {
                var candidate = _candidatesList[i];
                ref var repairTeams = ref _repairTeams.Get(candidate.RepairProduceEntity);
                if (repairTeams.FreeTeamsCount == 0)
                    continue;

                repairTeams.FreeTeamsCount--;
                var repairSpeed = _repairSpeed.Get(candidate.RepairProduceEntity).Value;
                _repairProcesses.Set(candidate.RepairableEntity, new(repairSpeed * deltaTime));
            }

            _candidatesList.Clear();
        }

        public void Dispose() { }

    }
}