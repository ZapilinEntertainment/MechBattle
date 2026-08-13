using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.PlayerData;

namespace ZE.MechBattle
{
    public class PlayerHandler
    {
        private PlayersList _playersList;
        private Stash<ControlledVehicleComponent> _controlledVehicles;
        private Stash<PlayerControlledTag> _playerControlledTags;

        [Inject]
        public PlayerHandler(IPlayersList playersList, World world)
        {
            _playersList = playersList as PlayersList;

            _controlledVehicles = world.GetStash<ControlledVehicleComponent>();
            _playerControlledTags = world.GetStash<PlayerControlledTag>();
        }

        public void AssumingVehicleControl(Entity vehicleEntity, PlayerKey playerKey )
        {
            // note: no vehicle switch functional yet

            var playerEntity = _playersList.GetPlayerEntity(playerKey);
            _controlledVehicles.Set(playerEntity, new(vehicleEntity));
            _playerControlledTags.Set(vehicleEntity);
        }
    
    }
}
