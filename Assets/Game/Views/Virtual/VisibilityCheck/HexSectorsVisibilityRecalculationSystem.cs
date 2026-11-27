using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexSectorsVisibilityRecalculationSystem : ILateSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<HexComponent> _hexComponents;
        private Stash<HexSectorsVisibilityMaskComponent> _hexVisibilityMasks;
        private readonly CameraController _cameraController;
        private readonly INavigationMap _map;

        [Inject]
        public HexSectorsVisibilityRecalculationSystem(INavigationMap map, CameraController cameraController)
        {
            _cameraController = cameraController;
            _map = map;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<HexComponent>().Build();
            _hexComponents = World.GetStash<HexComponent>();
            _hexVisibilityMasks = World.GetStash<HexSectorsVisibilityMaskComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            var edgeLenth = _map.HexEdgeLength;
            var job = new HexVisibilityCheckJob()
            {
                HexesFilter = _filter.AsNative(),
                CameraData = new CameraFrustrum2D(_cameraController.Camera, edgeLenth * 1.5f),
                HexComponents = _hexComponents.AsNative(),
                HexEdge = edgeLenth,
                HexVisibilityMasks = _hexVisibilityMasks.AsNative()                
            };
            World.JobHandle = job.Schedule();
        }

        public void Dispose()
        {
        }
    }
}