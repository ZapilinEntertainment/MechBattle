using System.Collections.Generic;
using VContainer;


namespace ZE.MechBattle.Projectiles
{
    public class CachedProjectileDataList : Dictionary<int, ProjectileData>, IProjectileDataList
    {
        private readonly ProjectilesData _projectileData;
        private readonly StringDataDictionary _stringDataDictionary;

        [Inject]
        public CachedProjectileDataList(ProjectilesData projectilesData, StringDataDictionary stringDataDictionary)
        {
            _stringDataDictionary = stringDataDictionary;
            _projectileData = projectilesData;
        }


        public bool TryGetProjectileData(int idKey, out ProjectileData data)
        {
            if (TryGetValue(idKey, out data))
                return true;

            var stringKey = _stringDataDictionary.GetStringByKey(idKey);
            return TryGetProjectileData(stringKey, idKey, out data);
        }

        public bool TryGetProjectileData(string stringKey, out ProjectileData data)
        {
            var idKey = _stringDataDictionary.StringToKey(stringKey);
            return TryGetProjectileData(stringKey, idKey, out data);
        }

        private bool TryGetProjectileData(string stringKey, int idKey, out ProjectileData data)
        {
            if (_projectileData.TryGetProjectileData(stringKey, out data))
            {
                Add(idKey, data);
                return true;
            }

#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"invalid projectile key: " + stringKey);
#endif

            data = default;
            return false;
        }
    }
}
