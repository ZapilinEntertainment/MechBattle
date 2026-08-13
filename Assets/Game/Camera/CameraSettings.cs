using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(CameraSettings), menuName = "Scriptable Objects/" + nameof(CameraSettings))]
    public class CameraSettings : ScriptableObject
    {
        [Serializable]
        public struct CameraSetup
        {
            public LayerMask CullingMask; 
        }

        [field: SerializeField] public SerializedDictionary<CameraMode, CameraSetup> _setups { get; private set; }

        public CameraSetup GetCameraSetup(CameraMode mode)
        {
            if (_setups.TryGetValue(mode, out var setup))
                return setup;

            return _setups[CameraMode.Default];
        }
    
    }
}
