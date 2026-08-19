using System.Collections.Generic;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Vfx;

namespace ZE.MechBattle
{
    // todo: make a generic logic for both ray effects and vfx
    public class RayEffectFactory
    {
        private readonly StringDataDictionary _stringDataDict;
        private readonly Dictionary<int, IRayEffectPlayer> _rayEffectPlayers = new();
        private readonly RayEffectView TEMP_defaultEffectView;
        private readonly Transform _poolsHost;

        [Inject]
        public RayEffectFactory(
            StringDataDictionary stringDataDictionary,
            [Key(DevelopConstants.DEFAULT_RAY_EFFECT_ID)] RayEffectView defaultEffectView)
        {
            _stringDataDict = stringDataDictionary;
            TEMP_defaultEffectView = defaultEffectView;

            _poolsHost = new GameObject("rayEffectsHost").transform;
        }

        public IDisposableRayEffectView Create(int effectId)
        {
            if (!_rayEffectPlayers.TryGetValue(effectId, out var rayEffectPlayer))
            {
                rayEffectPlayer = CreateEffectPlayer(effectId);
                _rayEffectPlayers.Add(effectId, rayEffectPlayer);
            }
            return rayEffectPlayer.GetRayEffect();
        }

        private IRayEffectPlayer CreateEffectPlayer(int effectId)
        {
            var stringKey = _stringDataDict.GetStringByKey(effectId);
            // todo: create some loading logic
            return new RayEffectsPool<RayEffectView>(TEMP_defaultEffectView, _poolsHost);
        }
    }
}
