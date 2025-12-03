using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ViewDestroyEffectSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ViewDestroyEffectComponent> _destroyEffects;
        private TransformAspectHandler _transformAspect;
        private readonly VfxRequestsBuilder _vfxRequestsBuilder;   

        public ViewDestroyEffectSystem(VfxRequestsBuilder vfxRequestsBuilder)
        {
            _vfxRequestsBuilder = vfxRequestsBuilder;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EntityDisposeTag>()
                .With<PositionComponent>()
                .With<ViewDestroyEffectComponent>()
                .Build();

            _destroyEffects = World.GetStash<ViewDestroyEffectComponent>();
            _transformAspect = new(World);
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                //UnityEngine.Debug.Log("destroy vfx call");
                var effectKey = _destroyEffects.Get(entity).EffectKey;
                _vfxRequestsBuilder.Build(new(effectKey), _transformAspect.GetPoint(entity));
            }
        }

        public void Dispose() { }
    }
}