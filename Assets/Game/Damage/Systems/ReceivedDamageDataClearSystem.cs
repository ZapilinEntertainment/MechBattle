using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ReceivedDamageDataClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<DamageReceivedComponent> _damageReceivedTag;
        private readonly ReceivedDamageList _receivedDamageList;

        [Inject]
        public ReceivedDamageDataClearSystem(ReceivedDamageList receivedDamageList)
        {
            _receivedDamageList = receivedDamageList;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<DamageReceivedComponent>().Build();
            _damageReceivedTag = World.GetStash<DamageReceivedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsNotEmpty())
                _damageReceivedTag.RemoveAll();

            if (!_receivedDamageList.IsEmpty)
                _receivedDamageList.Clear();
        }

        public void Dispose() { }
    }
}