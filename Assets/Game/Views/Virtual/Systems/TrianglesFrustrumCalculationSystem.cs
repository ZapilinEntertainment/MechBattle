using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglesFrustrumCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _viewsFilter;
        private Filter _trisFilter;
        private Stash<ViewVisibilityStateComponent> _visibilityStates;
        private readonly HashSet<IntTriangularPos> _visiblePositions = new(1024);
        private readonly CameraController _cameraController;

        [Inject]
        public TrianglesFrustrumCalculationSystem(CameraController cameraController)
        {
            _cameraController = cameraController;
        }

        public void OnAwake() 
        {
            _trisFilter = World.Filter
                .With<CellEntityComponent>()
                .Build();

            _viewsFilter = World.Filter
                .With<ViewVisibilityStateComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _visibilityStates = World.GetStash<ViewVisibilityStateComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {

        }

        public void Dispose()
        {

        }
    }
}