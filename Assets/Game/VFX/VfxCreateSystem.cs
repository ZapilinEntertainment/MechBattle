using UnityEngine;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VfxCreateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _vfxCreateFilter;
        private Stash<VfxRequestComponent> _vfxRequests;
        private Stash<VirtualPositionComponent> _vfxVirtualPositions;
        private Stash<VirtualRotationComponent> _vfxVirtualRotations;
        private readonly VfxManager _vfxManager;

        [Inject]
        public VfxCreateSystem(VfxManager vfxManager)
        {
            _vfxManager = vfxManager;
        }

        public void OnAwake() 
        {
            _vfxCreateFilter = World.Filter.With<VfxRequestComponent>().Build();
            _vfxRequests = World.GetStash<VfxRequestComponent>();
            _vfxVirtualPositions = World.GetStash<VirtualPositionComponent>();
            _vfxVirtualRotations = World.GetStash<VirtualRotationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_vfxCreateFilter.IsNotEmpty())
            {
                foreach (var request in _vfxCreateFilter)
                {
                    var data = _vfxRequests.Get(request);
                    var position = _vfxVirtualPositions.Get(request).Value;
                    var rotationComponent = _vfxVirtualRotations.Get(request, out var rotationSet);
                    var rotation = rotationSet ? rotationComponent.Value : (quaternion)UnityEngine.Random.rotationUniform;
                    _vfxManager.PlayEffect(data.Value, position, rotation);

                    World.RemoveEntity(request);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}