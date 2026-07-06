using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AttackTargetDefineSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private readonly TargetDefineProcess _targetDefineProcess;

        [Inject]
        public AttackTargetDefineSystem(
            IMovementCellsMap movementCellsMap, 
            INavigationMap map, 
            IPlayersList playersList, 
            PlayerRelations playerRelations,
            World world)
        {
            _targetDefineProcess = new(map, playersList, movementCellsMap, playerRelations, world);
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Without<AttackTargetComponent>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            // TODO: add search cd (for situations with no enemies in radius)

            if (_filter.IsNotEmpty())
            {
                World.JobHandle = _targetDefineProcess.Launch(_filter);                
            }                
        }

        public void Dispose()
        {
            _targetDefineProcess.Dispose();
        }
    }
}