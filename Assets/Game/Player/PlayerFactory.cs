using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.PlayerData
{
    public class PlayerFactory
    {
        private int _nextPlayerId = 1;
        private readonly World _world;
        private readonly MechCreateRequestsFactory _mechRequestsFactory;
        private readonly PlayersList _playersList;
        private readonly Stash<PlayerComponent> _playerComponents;

        [Inject]
        public PlayerFactory(
            World world, 
            MechCreateRequestsFactory mechRequestsFactory, 
            IPlayersList playersList)
        {
            _world = world;
            _playerComponents = world.GetStash<PlayerComponent>();

            _mechRequestsFactory = mechRequestsFactory;
            _playersList = playersList as PlayersList;
        }

        public Entity CreateLocalPlayer(RigidTransform spawnPoint)
        {
            var playerEntity = _world.CreateEntity();
            var id = _nextPlayerId++;
            _playerComponents.Add(playerEntity, new(id));
            var playerKey = new PlayerKey(id);
            _playersList.AddPlayerEntity(playerKey, playerEntity);

            _mechRequestsFactory.CreateRequest(new(playerKey, spawnPoint.pos, spawnPoint.rot, directControl: true));
            // todo: addEntityComponentAppearTracker for camera following
            return playerEntity;
        }
    }
}
