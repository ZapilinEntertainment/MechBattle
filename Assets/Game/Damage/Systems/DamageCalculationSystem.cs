using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // calculate damage by bonuses, resists or unit groups (ex.: friendly fire protection)
    public sealed class DamageCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Stash<CalculateDamageRequest> _calculateRequests;
        private Stash<ResultingDamageComponent> _resultingDamage;
        private Stash<HealthComponent> _healthComponents;
        private Filter _filter;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CalculateDamageRequest>()
                .Without<ResultingDamageComponent>()
                .Build();

            _calculateRequests = World.GetStash<CalculateDamageRequest>();
            _resultingDamage = World.GetStash<ResultingDamageComponent>();
            _healthComponents = World.GetStash<HealthComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
            {
                foreach (var request in _filter)
                {
                    if (!TryHandleRequest(request))
                        World.RemoveEntity(request);
                }
            }
        }

        public void Dispose() { }

        private bool TryHandleRequest(Entity request)
        {
            var requestBody = _calculateRequests.Get(request);
            var targetEntity = requestBody.Target;
            if (World.IsDisposed(targetEntity) || !_healthComponents.Has(targetEntity))
                return false;

            // some boost calculations will be here, or friendly fire checks
            // use damageParameters.Multiply

            _resultingDamage.Set(request, new() { DamageParameters = requestBody.Data});
            //UnityEngine.Debug.Log("resulting damage: " + resultingDamage);

            return true;
        }
    }
}