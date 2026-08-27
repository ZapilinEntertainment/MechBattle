using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInstanceSystem : EntityCreationSystemBase<MechInstanceRequestComponent, MechFactory>
    {
        private readonly PlayerHandler _playerHandler;
        private readonly MechHandler _mechHandler;

        public MechInstanceSystem(MechFactory factory, PlayerHandler playerHandler, MechHandler mechHandler) : base(factory)
        {
            _playerHandler = playerHandler;
            _mechHandler = mechHandler;
        }

        protected override bool TryExecuteRequest(Entity requestEntity)
        {
            var request = RequestsStash.Get(requestEntity);
            var mechEntity = Factory.Build(request.Position, request.Rotation);
            if (request.AssumingDirectControl)
                _playerHandler.AssumingVehicleControl(mechEntity, request.PlayerKey);
            _mechHandler.AssignMechPlayerAffinity(mechEntity, request.PlayerKey);
            return true;
        }
    }
}