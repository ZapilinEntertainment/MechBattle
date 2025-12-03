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
        private Filter _filter;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CalculateDamageRequest>()
                .Without<ResultingDamageComponent>()
                .Build();

            _calculateRequests = World.GetStash<CalculateDamageRequest>();
            _resultingDamage = World.GetStash<ResultingDamageComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
            {
                foreach (var request in _filter)
                {
                    HandleRequest(request);
                }
            }
        }

        public void Dispose() { }

        private void HandleRequest(Entity request)
        {
            var requestBody = _calculateRequests.Get(request);

            // some boost calculations will be here
            var resultingDamage = requestBody.Data.Value;
            _resultingDamage.Set(request, new() { Value = resultingDamage});
            //UnityEngine.Debug.Log("resulting damage: " + resultingDamage);
        }
    }
}