using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VirtualViewsCheckSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<VirtualViewComponent> _virtualViews;
        private Stash<VisibleVirtualViewTag> _visibilityTags;
        private readonly CameraController _cameraController;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public VirtualViewsCheckSystem(CameraController cameraController, TransformAspectHandler transformAspectHandler)
        {
            _cameraController = cameraController;
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<VirtualViewComponent>().Without<EntityDisposeTag>().Build();

            _virtualViews = World.GetStash<VirtualViewComponent>();
            _visibilityTags = World.GetStash<VisibleVirtualViewTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var cameraPosition = _cameraController.Camera.transform.position;
            foreach (var entity in _filter)
            {
                var virtualViewComponent = _virtualViews.Get(entity);
                var limit = virtualViewComponent.VisibilityLimit;
                if (limit == 0f)
                {
                    _visibilityTags.Set(entity);
                    continue;
                }
                   

                var size = VisualSizeCalculator.Calculate(virtualViewComponent.Scale);

                var position = _transformAspectHandler.GetPosition(entity);
                var distance = math.distance(position, cameraPosition);
                var viewCf = size / distance;
                if (viewCf > virtualViewComponent.VisibilityLimit)
                    _visibilityTags.Set(entity);
                else
                    _visibilityTags.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}