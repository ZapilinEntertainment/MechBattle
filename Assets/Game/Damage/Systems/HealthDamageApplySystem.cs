using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // doing applying effects, death effects and counting
    public sealed class HealthDamageApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<HealthComponent> _health;
        private Stash<EntityDisposeTag> _entityDisposeTag;

        private readonly VfxRequestsFactory _vfxRequestsFactory;
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly ReceivedDamageList _receivedDamageList;
        private readonly VfxKey _trampledVfxKey;

        [Inject]
        public HealthDamageApplySystem(
            VfxRequestsFactory vfxRequestsFactory, 
            StringDataDictionary stringDataDictionary, 
            TransformAspectHandler transformAspectHandler,
            ReceivedDamageList receivedDamageList)
        {
            _vfxRequestsFactory = vfxRequestsFactory;
            _transformAspectHandler = transformAspectHandler;
            _receivedDamageList = receivedDamageList;
            _trampledVfxKey = new VfxKey(stringDataDictionary.StringToKey(VfxConstants.TrampledExplosionId));
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<DamageReceivedComponent>()
                .With<HealthComponent>()
                .Build();

            _health = World.GetStash<HealthComponent>();
            _entityDisposeTag = World.GetStash<EntityDisposeTag>();

        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                ApplyDamage(entity, _receivedDamageList[entity]);
            }
        }

        public void Dispose() { }

        private void ApplyDamage(Entity target, IncomingDamageData damageData)
        {
            ref var healthComponent = ref _health.Get(target);
            var healthValue = math.clamp(healthComponent.CurrentValue - damageData.Volume,0, healthComponent.MaxValue);
            if (healthValue == 0f)
                OnEntityHealthIsZero(target, damageData);
            else
                healthComponent.CurrentValue = healthValue;
            //UnityEngine.Debug.Log($"health: {healthValue} / {healthComponent.MaxValue}");
        }

        private void OnEntityHealthIsZero(Entity entity, IncomingDamageData damageData)
        {
            _entityDisposeTag.Set(entity);
            if (damageData.Flags.HasFlag(ReceivedDamageFlag.Trampled))
                _vfxRequestsFactory.Build(_trampledVfxKey, _transformAspectHandler.GetPosition(entity));
        }
    }
}