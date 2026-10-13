using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.MechBuilding;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInstanceSystem : RequestHandleSystemBase<MechInstanceRequestComponent>
    {
        private Stash<CreationRequestResultComponent> _results;
        private readonly PlayerHandler _playerHandler;
        private readonly MechFactory _mechFactory;
        private readonly MechBotHandler _mechBotHandler;
        

        public MechInstanceSystem(MechFactory factory, PlayerHandler playerHandler, MechBotHandler mechBotHandler)
        {
            _mechFactory = factory;
            _playerHandler = playerHandler;
            _mechBotHandler = mechBotHandler;
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _results = World.GetStash<CreationRequestResultComponent>();
        }

        protected override bool TryHandleRequest(Entity requestEntity)
        {
            var request = GetRequestComponent(requestEntity);
            var mechEntity = _mechFactory.Build(request.Position, request.Rotation);
            if (request.AssumingDirectControl)
            {
                _playerHandler.AssumingVehicleControl(mechEntity, request.PlayerKey);
            }                
            else
            {
                _mechBotHandler.ApplyBotControls(mechEntity, request.PlayerKey);
            }

            
            _results.Set(requestEntity, new(mechEntity));
            return true;
        }
    }
}