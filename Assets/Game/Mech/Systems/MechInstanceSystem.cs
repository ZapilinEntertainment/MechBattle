using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInstanceSystem : EntityCreationSystemBase<MechInstanceRequestComponent, MechFactory>
    {
        private readonly PlayerHandler _playerHandler;
        private Stash<PlayerAffiliationComponent> _affiliations;

        public MechInstanceSystem(MechFactory factory, PlayerHandler playerHandler) : base(factory)
        {
            _playerHandler = playerHandler;
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
        }

        protected override bool TryExecuteRequest(Entity requestEntity)
        {
            var request = RequestsStash.Get(requestEntity);
            var mechEntity = Factory.Build(request.Position, request.Rotation);

            _affiliations.Set(mechEntity, new(request.PlayerKey));
            if (request.AssumingDirectControl)
                _playerHandler.AssumingVehicleControl(mechEntity, request.PlayerKey);
            return true;
        }
    }
}