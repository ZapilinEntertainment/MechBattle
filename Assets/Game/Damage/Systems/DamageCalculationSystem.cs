using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // calculate damage by bonuses, resists or unit groups (ex.: friendly fire protection)
    public sealed class DamageCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Stash<DamageReceivedComponent> _damageReceivedTags;
        private readonly DamageRequestsList _damageRequestsList;
        private readonly ReceivedDamageList _receivedDamageList;

        [Inject]
        public DamageCalculationSystem(DamageRequestsList requestsList, ReceivedDamageList receivedDamageList)
        {
            _damageRequestsList = requestsList;
            _receivedDamageList = receivedDamageList;
        }

        public void OnAwake() 
        {
            _damageReceivedTags = World.GetStash<DamageReceivedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_damageRequestsList.IsEmpty)
                return;

            foreach (var request in _damageRequestsList)
            {
                var target = request.Target;
                if (World.IsDisposed(target))
                    continue;

                // some boost calculations will be here, or friendly fire checks
                // use damageParameters.Multiply

                var flags = ReceivedDamageFlag.None;
                if (request.DamageApplyParameters.DamageType == DamageType.Trampling)
                    flags |= ReceivedDamageFlag.Trampled;
                
                _receivedDamageList.Add(target, new() { Volume = request.DamageApplyParameters.Value, Flags =flags });
                _damageReceivedTags.Set(target);
            }
            _damageRequestsList.Clear();
        }

        public void Dispose() { }
    }
}