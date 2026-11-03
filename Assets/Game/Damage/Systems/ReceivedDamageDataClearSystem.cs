using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ReceivedDamageDataClearSystem : TagsClearSystem<DamageReceivedComponent>
    {
        private readonly ReceivedDamageList _receivedDamageList;

        [Inject]
        public ReceivedDamageDataClearSystem(ReceivedDamageList receivedDamageList)
        {
            _receivedDamageList = receivedDamageList;
        }

        public override void OnUpdate(float deltaTime) 
        {
            base.OnUpdate(deltaTime);

            if (!_receivedDamageList.IsEmpty)
                _receivedDamageList.Clear();
        }
    }
}