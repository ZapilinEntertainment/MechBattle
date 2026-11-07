using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle
{
    public class MaterialsDepot
    {
        private readonly Dictionary<int, Material> _materials = new(8);
        private readonly StringDataDictionary _stringDataDictionary;
        private int _nextTempIndex = -1;

        [Inject]
        public MaterialsDepot(StringDataDictionary stringDataDictionary)
        {
            _stringDataDictionary = stringDataDictionary;
        }

        public bool TryGetMaterialById(int idKey, out Material material) => _materials.TryGetValue(idKey, out material);
        public Material GetMaterialById(int idKey) => _materials.TryGetValue(idKey, out var material) ? material : default;
        public Material GetMaterialByKey(string key)
        {
            var idKey = GetMaterialIdKey(key);
            return _materials.TryGetValue(idKey, out var material) ? material : default;
        }

        public int GetMaterialIdKey(string key) => _stringDataDictionary.StringToKey(key);

        public int RegisterMaterial(string key, Material material)
        {
            var idKey = _stringDataDictionary.StringToKey(key);
            _materials.Add(idKey, material);
            //UnityEngine.Debug.Log($"{key} -> {idKey}");
            return idKey;
        }
    
        public IDisposable RegisterTemporalMaterial(Material material, out int materialId)
        {
            var id = _nextTempIndex--;
            _materials.Add(id, material);
            materialId = id;
            return Disposable.Create(() => DisposeTemporalMaterial(id));
        }

        private void DisposeTemporalMaterial(int key)
        {
            var material = _materials[key];
            GameObject.Destroy(material);
        }
    }
}
