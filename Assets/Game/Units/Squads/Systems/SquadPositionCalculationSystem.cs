using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;

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
        private Stash<SquadComponent> _squadComponents;
        private Stash<SquadMemberComponent> _squadMembers;

        public void OnAwake() 
        {
            _squadsFilter = World.Filter.With<SquadComponent>().Build();
            _squadMembersFilter = World.Filter.With<SquadMemberComponent>().Build();            

            _positions = World.GetStash<PositionComponent>();
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
            };
            
            World.JobHandle = job.Schedule();
#if MORPEH_JOB_TRACKING
            UnityEngine.Debug.Log("squad position calculation job");
#endif
        }

        public void Dispose() { }
    }
}