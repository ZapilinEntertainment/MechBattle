using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // doing applying effects, death effects and counting
    public sealed class DamageApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<CalculateDamageRequest> _requestInfo;
        private Stash<ResultingDamageComponent> _resultingDamage;
        private Stash<HealthComponent> _health;
        private Stash<EntityDisposeTag> _entityDisposeTag;

        private readonly VfxRequestsFactory _vfxRequestsFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly VfxKey _trampledVfxKey;

        [Inject]
        public DamageApplySystem(VfxRequestsFactory vfxRequestsFactory, StringDataDictionary stringDataDictionary, TransformAspectHandler transformAspectHandler)
        {
            _vfxRequestsFactory = vfxRequestsFactory;
            _transformAspectHandler = transformAspectHandler;
            _trampledVfxKey = new VfxKey(stringDataDictionary.StringToKey(VfxConstants.TrampledExplosionId));
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CalculateDamageRequest>()
                .With<ResultingDamageComponent>()
                .Build();

            _requestInfo = World.GetStash<CalculateDamageRequest>();
            _resultingDamage = World.GetStash<ResultingDamageComponent>();
            _health = World.GetStash<HealthComponent>();
            _entityDisposeTag = World.GetStash<EntityDisposeTag>();

        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
            {
                foreach (var request in _filter)
                {
                    // no need to do checks: calculation system did it

                    var target = _requestInfo.Get(request).Target;
                    var damageParameters = _resultingDamage.Get(request).DamageParameters;
                    ApplyDamage(target, damageParameters);
                    World.RemoveEntity(request);
                }
            }
        }

        public void Dispose() { }

        private void ApplyDamage(Entity target, DamageApplyParameters damageParameters)
        {
            ref var healthComponent = ref _health.Get(target);
            var healthValue = math.clamp(healthComponent.CurrentValue - damageParameters.Value,0, healthComponent.MaxValue);
            if (healthValue == 0f)
                OnEntityHealthIsZero(target, damageParameters);
            else
                healthComponent.CurrentValue = healthValue;
            //UnityEngine.Debug.Log($"health: {healthValue} / {healthComponent.MaxValue}");
        }

        private void OnEntityHealthIsZero(Entity entity, DamageApplyParameters damageParameters)
        {
            _entityDisposeTag.Set(entity);
            if (damageParameters.DamageType == DamageType.Trampling)
                _vfxRequestsFactory.Build(_trampledVfxKey, _transformAspectHandler.GetPosition(entity));
        }
    }
}