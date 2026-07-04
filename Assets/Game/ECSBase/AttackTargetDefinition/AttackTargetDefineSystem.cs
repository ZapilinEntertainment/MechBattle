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
        private readonly MovementCellsMap _movementCellsMap;
        private readonly TargetDefineProcess _targetDefineProcess;

        [Inject]
        public AttackTargetDefineSystem(
            MovementCellsMap movementCellsMap, 
            INavigationMap map, 
            IPlayersList playersList, 
            PlayerRelations playerRelations,
            World world)
        {
            _movementCellsMap = movementCellsMap;
            _targetDefineProcess = new(map, playersList, playerRelations, world);
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<TargetSearchRadiusComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            World.JobHandle = _targetDefineProcess.Launch(_filter);
        }

        public void Dispose()
        {
            _targetDefineProcess.Dispose();
        }
    }
}