using System.Collections.Generic;

namespace ZE.MechBattle
{
    public interface IPlayersList
    {
        int Count { get; }
    }
    public class PlayersList : IPlayersList
    {
        //private readonly Dictionary<int, PlayerData> _players = new();
    
        public int Count => 2;
    }
}
