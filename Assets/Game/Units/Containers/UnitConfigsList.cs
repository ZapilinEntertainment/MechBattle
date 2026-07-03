using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public interface IUnitConfigsList
    {
        bool TryGetConfig(UnitKey key, out UnitConfig config);
    }

    public class UnitConfigsList : IUnitConfigsList
    {
        private readonly StringDataDictionary _stringDictionary;
        private readonly Dictionary<int, UnitConfig> _dictionary = new();

        [Inject]
        public UnitConfigsList(StringDataDictionary stringDictionary)
        {
            _stringDictionary = stringDictionary;
        }

        public void AddConfig(UnitConfig config) 
        { 
            var name = config.name;
            var id = _stringDictionary.GetStringKey(name);
            _dictionary.Add(id, config);
        }

        public bool TryGetConfig(UnitKey key, out UnitConfig config) => _dictionary.TryGetValue(key.Id, out config);

    }
}
