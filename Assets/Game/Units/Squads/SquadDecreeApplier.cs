using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Units.Squads
{
    public class SquadDecreeApplier
    {
        private readonly INavigationMap _map;
        private readonly SquadMembersIterator _squadMembersIterator;
        private readonly MoveTargetApplier _moveTargetApplier;

        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<SquadComponent> _squads;
        private readonly Stash<SquadDecreeComponent> _decreeComponents;
        private readonly Stash<UnhandledDecreeTag> _unhandledDecrees;

        [Inject]
        public SquadDecreeApplier(World world, INavigationMap map, SquadMembersIterator squadMembersIterator, MoveTargetApplier moveTargetApplier)
        {
            _map = map;
            _squadMembersIterator = squadMembersIterator;
            _moveTargetApplier = moveTargetApplier;

            _moveTargets = world.GetStash<MoveTargetComponent>();
            _squads = world.GetStash<SquadComponent>();

            _decreeComponents = world.GetStash<SquadDecreeComponent>();
            _unhandledDecrees = world.GetStash<UnhandledDecreeTag>();
        }

        public void SetSquadMoveDecree(Entity squad, IntTriangularPos tripos)
        {
            _moveTargets.Set(squad, new(tripos, _map));
            _decreeComponents.Set(squad, new() { Type = SquadDecreeType.Move });
            _unhandledDecrees.Set(squad);
        }

        public void ApplyMovementDecree(Entity squadEntity)
        {
            var targetComponent = _moveTargets.Get(squadEntity);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(targetComponent.WorldPos, _map.TriangleHeight, targetComponent.TriangularPos);

            var elementsCount = _squads.Get(squadEntity).MembersCount;
            var minHexRadius = TriangularMath.GetTrianglesCountInHex(elementsCount);
            using var coordsConverterData = FlattenedHexCoordsConverter.CreateCoordsConverter(Unity.Collections.Allocator.Temp, virtualHexCenter, _map.Settings, out var coordsConverter);

            foreach (var (memberEntity, memberIndex) in _squadMembersIterator.GetNextSquadMember(squadEntity))
            {
                var tripos = coordsConverter.IndexToTriangular(memberIndex);
                _moveTargetApplier.SetMoveTarget(memberEntity, tripos);
            }
        }
    
    }
}
