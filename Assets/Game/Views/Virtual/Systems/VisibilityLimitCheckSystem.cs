using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VisibilityLimitCheckSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<VirtualViewComponent> _virtualViews;
        private Stash<ViewVisibilityStateComponent> _visibilities;
        private readonly CameraController _cameraController;
        private readonly TransformAspectHandler _transformAspectHandler;
        private const VisibilityStateFlag VISIBILITY_LIMIT_REACHED_MASK = ~VisibilityStateFlag.InVisibilityLimit;

        [Inject]
        public VisibilityLimitCheckSystem(CameraController cameraController, TransformAspectHandler transformAspectHandler)
        {
            _cameraController = cameraController;
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<VirtualViewComponent>()
                .With<ViewVisibilityStateComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _virtualViews = World.GetStash<VirtualViewComponent>();
            _visibilities = World.GetStash<ViewVisibilityStateComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var cameraPosition = _cameraController.Camera.transform.position;
            foreach (var entity in _filter)
            {
                var virtualViewComponent = _virtualViews.Get(entity);
                var limit = virtualViewComponent.VisibilityLimit;
                ref var visibilityComponent = ref _visibilities.Get(entity);

                if (limit == 0f)
                {
                    visibilityComponent.Value |= VisibilityStateFlag.InVisibilityLimit;
                    continue;
                }
                   

                var size = VisualSizeCalculator.Calculate(virtualViewComponent.Scale);

                var position = _transformAspectHandler.GetPosition(entity);
                var distance = math.distance(position, cameraPosition);
                var viewCf = size / distance;
                if (viewCf > virtualViewComponent.VisibilityLimit)
                    visibilityComponent.Value |= VisibilityStateFlag.InVisibilityLimit;
                else
                    visibilityComponent.Value &= VISIBILITY_LIMIT_REACHED_MASK;
            }
        }

        public void Dispose()
        {

        }
    }
}