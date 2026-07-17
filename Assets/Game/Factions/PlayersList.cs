using System.Collections.Generic;
using UnityEngine;

namespace ZE.MechBattle
{
    public interface IPlayersList
    {
        int Count { get; }
        Color GetPlayerColor(PlayerKey playerKey);
    }
    public class PlayersList : IPlayersList
    {
        private struct PlayerData
        {
            public Color Color;
        }

        private readonly Dictionary<int, PlayerData> _players = new();
    
        public int Count => 2;

        public PlayersList() 
        {
            _players.Add(0, new PlayerData() { Color = Color.blue});
            _players.Add(1, new PlayerData() { Color = Color.red });
        }

        public Color GetPlayerColor(PlayerKey playerKey) => _players.TryGetValue(playerKey.Id, out var playerData) ? playerData.Color : Color.white;
    }
}
