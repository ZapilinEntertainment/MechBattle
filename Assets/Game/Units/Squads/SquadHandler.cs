using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle
{
    public class SquadHandler : IDisposable
    {
        private FlattenedHexCoordsConverter _coordsConverter;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly Stash<SquadUpdateRequiredTag> _changedTags;
        private readonly Stash<SquadMemberComponent> _squadMembers;
        private readonly Stash<SquadComponent> _squadComponents;
        private readonly Stash<TriangularPosComponent> _triangularPositions;
        private readonly Filter _squadsFilter;

        private readonly SquadMembersIterator _squadMembersIterator;
        private readonly MoveTargetApplier _moveTargetApplier;
        private readonly INavigationMap _navigationMap;      
        private readonly IDisposable _disposable;

        [Inject]
        public SquadHandler(World world,  TransformAspectHandler transformAspectHandler, INavigationMap navigationMap, MoveTargetApplier moveTargetApplier)
        {
            _transformAspectHandler = transformAspectHandler;
            _navigationMap = navigationMap;
            _moveTargetApplier = moveTargetApplier;

            _changedTags = world.GetStash<SquadUpdateRequiredTag>();
            _squadMembers = world.GetStash<SquadMemberComponent>();
            _squadComponents = world.GetStash<SquadComponent>();
            _triangularPositions = world.GetStash<TriangularPosComponent>();

            var membersFilter = world.Filter.With<SquadMemberComponent>().Build();
            _squadMembersIterator = new(membersFilter, _squadMembers, _squadComponents);

            _squadsFilter = world.Filter.With<SquadComponent>().Build();

            _disposable = FlattenedHexCoordsConverter.CreateCoordsConverter(Unity.Collections.Allocator.Persistent, default, _navigationMap.Settings, out _coordsConverter);
        }

        public IEnumerable<(Entity memberEntity, int memberIndex)> GetNextSquadMember(Entity squadEntity) => 
            _squadMembersIterator.GetNextSquadMember(squadEntity);

        public IEnumerable<Entity> GetNextSquad()
        {
            foreach (var squadEntity in _squadsFilter)
            {
                yield return squadEntity;
            }
        }

        public float3 GetSquadPosition(Entity squadEntity) => _transformAspectHandler.GetPosition(squadEntity);

        public void AssignEntityToSquad(Entity entity, Entity squadEntity)
        {
            ref var squadComponent = ref _squadComponents.Get(squadEntity);
            _squadMembers.Set(entity, new(squadEntity, squadComponent.MembersCount));
            squadComponent.MembersCount += 1;
            _changedTags.Set(squadEntity);
        }

        public bool RecalculateMembersPositions(Entity entity, float3 worldPos, IntTriangularPos tripos)
        {
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(worldPos, _navigationMap.TriangleHeight, tripos);
            _coordsConverter = _coordsConverter.ChangeHexCenter(virtualHexCenter);

            var elementsCount = _squadComponents.Get(entity).MembersCount;
            var minHexRadius = TriangularMath.GetTrianglesCountInHex(elementsCount);

            var membersCount = 0;
            var positionsMatch = 0;
            foreach (var (memberEntity, memberIndex) in GetNextSquadMember(entity))
            {
                membersCount++;
                var currentTripos = _triangularPositions.Get(memberEntity).Value;
                var triangleDistance = TriangularMath.CalculateDistance(currentTripos, virtualHexCenter);
                if (triangleDistance <= minHexRadius)
                {
                    positionsMatch++;
                    continue;
                }

                var targetTripos = _coordsConverter.IndexToTriangular(memberIndex);
                _moveTargetApplier.SetMoveTarget(memberEntity, targetTripos);
            }

            return membersCount == positionsMatch;
        }

        public void Dispose() => _disposable.Dispose();
    }
}
