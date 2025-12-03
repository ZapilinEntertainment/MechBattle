using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Vfx;

namespace ZE.MechBattle
{
    public interface IEffectPlayer : IDisposable
    {
        void Play(float3 pos, quaternion rot);
    }

    public class VfxManager : IDisposable
    {
        private readonly VfxData _vfxData;
        private readonly VfxEffectPlayersFactory _vfxFactory;
        private readonly StringDataDictionary _strDict;
        private readonly Dictionary<int, IEffectPlayer> _players = new();

        [Inject]
        public VfxManager(StringDataDictionary strDict, VfxData vfxData, VfxEffectPlayersFactory vfxFactory)
        {
            _strDict = strDict;
            _vfxData = vfxData;
            _vfxFactory = vfxFactory;
        }

        public void PlayEffect(VfxKey key, float3 position, quaternion rotation)
        {
            if (!_players.TryGetValue(key.IdKey, out var player))
            {
                var str = _strDict.GetStringByKey(key.IdKey);
                if (_vfxData.TryGetVfxData(str, out var data))
                {
                    player = _vfxFactory.CreateEffect(data);
                    _players.Add(key.IdKey, player);
                }
#if UNITY_EDITOR
                else Debug.LogWarning(key.IdKey + " effect data not found");
#endif
            }
            player?.Play(position, rotation);
        }

        public void Dispose()
        {
            foreach (var player in _players.Values) player.Dispose();
            _players.Clear();
        }
    }
}
