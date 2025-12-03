using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

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
                    var target = _requestInfo.Get(request).Target;
                    if (!World.IsDisposed(target))
                    {
                        var damage = _resultingDamage.Get(request).Value;
                        ApplyDamage(target, damage);
                    }
                    World.RemoveEntity(request);
                }
            }
        }

        public void Dispose() { }

        private void ApplyDamage(Entity target, float damage)
        {
            ref var healthComponent = ref _health.Get(target);
            var healthValue = math.clamp(healthComponent.CurrentValue - damage,0, healthComponent.MaxValue);
            if (healthValue == 0f)
                OnEntityHealthIsZero(target);
            else
                healthComponent.CurrentValue = healthValue;
            //UnityEngine.Debug.Log($"health: {healthValue} / {healthComponent.MaxValue}");
        }

        private void OnEntityHealthIsZero(Entity entity)
        {
            _entityDisposeTag.Add(entity);
        }
    }
}