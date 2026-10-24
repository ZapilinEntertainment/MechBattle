using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public interface IUnitConfigsList
    {
        bool TryGetUnitConfig(UnitKey key, out UnitConfig unitConfig);
    }

    public class UnitConfigsList : IUnitConfigsList
    {
        private readonly StringDataDictionary _stringDictionary;
        private readonly Dictionary<int, UnitConfig> _baseConfigs = new();

        [Inject]
        public UnitConfigsList(StringDataDictionary stringDictionary)
        {
            _stringDictionary = stringDictionary;
        }

        public void AddConfig(UnitConfig config) 
        { 
            var name = config.name;
            var id = _stringDictionary.StringToKey(name);
            _baseConfigs.Add(id, config);
            //UnityEngine.Debug.Log($"registered config {name} as id {id}, keys count: {_dictionary.Count}");
        }

        public bool TryGetUnitConfig(UnitKey key, out UnitConfig unitConfig) => _baseConfigs.TryGetValue(key.Id, out unitConfig);
    }
}
