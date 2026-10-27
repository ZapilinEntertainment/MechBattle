using UnityEngine;
using VContainer;
using TriInspector;

namespace ZE.MechBattle
{
    public class LevelPlayerConfigs : ScopeConfigurationComponent
    {
        [ValidateInput(nameof(ValidatePlayersCount))]
        [SerializeField] private PlayerConfig[] _configs;
        [SerializeField] private int _localPlayerIndex = 0;

        public PlayerKey LocalPlayerKey => new(_localPlayerIndex);

        public override void OnConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new PlayerConfigsList(_configs, LocalPlayerKey));
        }

        public TriValidationResult ValidatePlayersCount()
        {
            if (_configs != null && _configs.Length > FactionConstants.MAX_PLAYERS)
            {
                return TriValidationResult.Error("Max players count reached. Increase FactionConstants.MAX_PLAYERS");
            }

            return TriValidationResult.Valid;
        }
    }
}
