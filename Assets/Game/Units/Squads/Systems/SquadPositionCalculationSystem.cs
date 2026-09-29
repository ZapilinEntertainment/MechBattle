using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SquadPositionCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _squadsFilter;
        private Filter _squadMembersFilter;
        private Stash<PositionComponent> _positions;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<SquadComponent> _squadComponents;
        private Stash<SquadMemberComponent> _squadMembers;
        private readonly float HexEdgeLength;

        [Inject]
        public SquadPositionCalculationSystem(INavigationMap map)
        {
            HexEdgeLength = map.HexEdgeLength;
        }

        public void OnAwake() 
        {
            _squadsFilter = World.Filter.With<SquadComponent>().Build();
            _squadMembersFilter = World.Filter.With<SquadMemberComponent>().Build();

            _positions = World.GetStash<PositionComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _squadComponents = World.GetStash<SquadComponent>();
            _squadMembers = World.GetStash<SquadMemberComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_squadsFilter.IsEmpty())
                return;

            var job = new SquadPositionCalculationJob()
            {
                SquadMembersFilter = _squadMembersFilter.AsNative(),
                SquadsFilter = _squadsFilter.AsNative(),
                Positions = _positions.AsNative(),
                SquadComponents = _squadComponents.AsNative(),
                SquadMembers = _squadMembers.AsNative(),
                HexCoords = _hexCoords.AsNative(),
                HexEdgeLength = HexEdgeLength
            };
            World.JobHandle = job.Schedule();
        }

        public void Dispose()
        {

        }
    }
}