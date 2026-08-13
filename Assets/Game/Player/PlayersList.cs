using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public interface IPlayersList
    {
        int Count { get; }
        Color GetPlayerColor(PlayerKey playerKey);
        Entity GetPlayerEntity(PlayerKey playerKey);
    }
}

namespace ZE.MechBattle.PlayerData
{
    
    public class PlayersList : IPlayersList
    {
        private struct PlayerData
        {
            public Entity Entity;
            public Color Color;
        }

        private readonly Dictionary<PlayerKey, PlayerData> _players = new();
        private readonly Color[] _colors = new Color[MAX_PLAYERS]
        {
            Color.white,
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow,
            Color.purple,
            Color.pink,
            Color.orange
        };
        private const int MAX_PLAYERS = 8;    
        public int Count => MAX_PLAYERS;

        public Color GetPlayerColor(PlayerKey playerKey) => _players.TryGetValue(playerKey, out var playerData) ? playerData.Color : Color.white;
        public Entity GetPlayerEntity(PlayerKey playerKey) => _players.TryGetValue(playerKey, out var playerData) ? playerData.Entity : default;

        public void AddPlayerEntity(PlayerKey playerKey, Entity entity)
        {
            var data = new PlayerData()
            {
                Entity = entity,
                Color = _colors[playerKey.Id]
            };

            _players.Add(playerKey, data);
        }
    }
}
