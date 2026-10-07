using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.PlayerData;

namespace ZE.MechBattle
{
    public class PlayerHandler
    {
        private readonly MechHandler _mechHandler;
        private readonly PlayersList _playersList;
        private readonly MechWeaponsHandler _weaponsHandler;
        private readonly Stash<ControlledVehicleComponent> _controlledVehicles;
        private readonly Stash<PlayerControlledTag> _playerControlledTags;

        [Inject]
        public PlayerHandler(IPlayersList playersList, World world, MechHandler mechHandler, MechWeaponsHandler weaponsHandler)
        {
            _mechHandler = mechHandler;
            _playersList = playersList as PlayersList;
            _weaponsHandler = weaponsHandler;

            _controlledVehicles = world.GetStash<ControlledVehicleComponent>();
            _playerControlledTags = world.GetStash<PlayerControlledTag>();
        }

        public void AssumingVehicleControl(Entity vehicleEntity, PlayerKey playerKey )
        {
            // note: no vehicle switch functional yet

            var playerEntity = _playersList.GetPlayerEntity(playerKey);
            _controlledVehicles.Set(playerEntity, new(vehicleEntity));
            _playerControlledTags.Set(vehicleEntity);

            _mechHandler.AssignMechPlayerAffinity(vehicleEntity, playerKey);
            _weaponsHandler.RemoveWeaponsBotControlComponents(vehicleEntity);
        }
    
    }
}
