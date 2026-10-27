using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public interface IPlayersList
    {
        int Count { get; }
        Entity GetPlayerEntity(PlayerKey playerKey);
        Entity GetLocalPlayerEntity();
    }
}

namespace ZE.MechBattle.PlayerData
{
    
    public class PlayersList : IPlayersList
    {
        public int Count => _configsList.Count;
        private readonly Dictionary<PlayerKey, Entity> _players = new();
        private readonly PlayerFactory _playerFactory;
        private readonly PlayerConfigsList _configsList;

        [Inject]
        public PlayersList(PlayerFactory playerFactory, PlayerConfigsList playerConfigsList)
        {
            _playerFactory = playerFactory;
            _configsList = playerConfigsList;
        }

        public Entity GetPlayerEntity(PlayerKey key) => GetOrCreatePlayer(key);
        public Entity GetLocalPlayerEntity() => GetOrCreatePlayer(_configsList.LocalPlayerKey);

        private Entity GetOrCreatePlayer(PlayerKey key)
        {
            if (_players.TryGetValue(key, out var playerEntity))
                return playerEntity;

            var id = key.Id;
            if (!_configsList.TryGetPlayerConfig(id, out var playerConfig))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError($"no player {id} config presented");
                return default;
#endif
            }

            playerEntity = _playerFactory.CreatePlayer(key, playerConfig);
            _players.Add(key, playerEntity);
            return playerEntity;
        } 
        
    }
}
