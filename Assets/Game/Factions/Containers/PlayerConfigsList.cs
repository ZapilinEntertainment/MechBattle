namespace ZE.MechBattle
{
    public class PlayerConfigsList
    {
        public readonly int Count;
        public readonly PlayerKey LocalPlayerKey;
        private readonly PlayerConfig[] _configs;
    
        public PlayerConfigsList(PlayerConfig[] configs, PlayerKey localPlayerKey)
        {
            Count = configs.Length;
            _configs = configs;
            LocalPlayerKey = localPlayerKey;
            
#if UNITY_EDITOR
            if (Count == 0)
                UnityEngine.Debug.LogError("no player configs presented");            
#endif
        }

        public bool TryGetPlayerConfig(int playerId, out PlayerConfig config)
        {
            if ((uint)playerId < (uint)_configs.Length)
            {
                config = _configs[playerId];
                return true;
            }
            else
            {
                config = _configs[0];
                return false;
            }
        }
    }
}
