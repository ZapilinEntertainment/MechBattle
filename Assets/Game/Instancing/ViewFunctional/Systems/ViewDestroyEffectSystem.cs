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
        private readonly VfxRequestsFactory _vfxRequestsBuilder;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public ViewDestroyEffectSystem(VfxRequestsFactory vfxRequestsBuilder, TransformAspectHandler transformAspectHandler)
        {
            _vfxRequestsBuilder = vfxRequestsBuilder;
            _transformAspectHandler = transformAspectHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EntityDisposeTag>()
                .With<PositionComponent>()
                .With<ViewDestroyEffectComponent>()
                .Build();

            _destroyEffects = World.GetStash<ViewDestroyEffectComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                //UnityEngine.Debug.Log("destroy vfx call");
                var effectKey = _destroyEffects.Get(entity).EffectKey;
                _vfxRequestsBuilder.Build(new(effectKey), _transformAspectHandler.GetPoint(entity));
            }
        }

        public void Dispose() { }
    }
}