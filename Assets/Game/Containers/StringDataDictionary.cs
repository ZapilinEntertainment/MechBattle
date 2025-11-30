using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZE.MechBattle
{
    public class StringDataDictionary : IDisposable
    {
        private Dictionary<string, int> _stringToKey = new();
        private Dictionary<int, string> _keyToString = new();
        private int _nextKey = 0;

        public int GetStringKey(string str)
        {
            if (_stringToKey.TryGetValue(str, out var key))
                return key;

            return RegisterString(str);
        }

        public bool TryGetStringByKey(int key, out string str) => _keyToString.TryGetValue(key, out str);

        public string GetStringByKey(int key)
        {
            if (_keyToString.TryGetValue(key, out var str))
                return str;

            return string.Empty;
        }

        private int RegisterString(string str)
        {
            var key = ++_nextKey;
            _stringToKey.Add(str, key);
            _keyToString.Add(key,str);
            return key;
        }

        public void Dispose()
        {
            _stringToKey.Clear();
            _keyToString.Clear();
        }
    
    }
}
