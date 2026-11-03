using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.PlayerData
{
    public class PlayerFactory
    {
        private readonly World _world;
        private readonly Stash<PlayerComponent> _playerComponents;
        private readonly Stash<FactionColorComponent> _colors;

        [Inject]
        public PlayerFactory(World world)
        {
            _world = world;           

            _playerComponents = world.GetStash<PlayerComponent>();
            _colors = world.GetStash<FactionColorComponent>();
        }

        public Entity CreatePlayer(PlayerKey key, PlayerConfig playerConfig)
        {
            var playerEntity = _world.CreateEntity();
            _playerComponents.Add(playerEntity, new(key));
            _colors.Add(playerEntity, new(playerConfig.Color));

            return playerEntity;
        }
    }
}
