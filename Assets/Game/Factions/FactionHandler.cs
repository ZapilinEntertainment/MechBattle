using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class FactionHandler
    {
        private readonly IPlayersList _playersList;
        private readonly PlayerConfigsList _configsList;
        private readonly Stash<FactionColorComponent> _factionColors;

        [Inject]
        public FactionHandler(IPlayersList playersList, World world, PlayerConfigsList playerConfigs)
        {
            _playersList = playersList;
            _configsList = playerConfigs;
            _factionColors = world.GetStash<FactionColorComponent>();
        }

        public Entity GetLocalPlayerEntity() => _playersList.GetLocalPlayerEntity();
        public PlayerKey GetLocalPlayerKey() => _configsList.LocalPlayerKey;

        public Color GetFactionColor(PlayerKey playerKey) 
        {
            if (_playersList.TryGetPlayerEntity(playerKey, out var playerEntity))
                return _factionColors.Get(playerEntity).Color;
            else
                return Color.white;
        } 
    
    }
}
