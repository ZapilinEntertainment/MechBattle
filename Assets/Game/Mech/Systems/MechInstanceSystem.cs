using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInstanceSystem : RequestHandleSystemBase<MechInstanceRequestComponent>
    {
        private readonly PlayerHandler _playerHandler;
        private readonly MechHandler _mechHandler;
        private readonly MechFactory _mechFactory;

        public MechInstanceSystem(MechFactory factory, PlayerHandler playerHandler, MechHandler mechHandler)
        {
            _mechFactory = factory;
            _playerHandler = playerHandler;
            _mechHandler = mechHandler;
        }

        protected override bool TryHandleRequest(MechInstanceRequestComponent request)
        {
            var mechEntity = _mechFactory.Build(request.Position, request.Rotation);
            if (request.AssumingDirectControl)
                _playerHandler.AssumingVehicleControl(mechEntity, request.PlayerKey);
            _mechHandler.AssignMechPlayerAffinity(mechEntity, request.PlayerKey);
            return true;
        }
    }
}