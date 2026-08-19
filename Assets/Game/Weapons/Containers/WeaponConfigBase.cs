using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    public abstract class WeaponConfigBase : ScriptableObject
    {
        [field: SerializeField] public float MinRange { get; private set; }
        [field: SerializeField, Range(0, 1)] public float RecommendedRangePc { get; private set; } = 0.8f;
        [field: SerializeField] public float MaxRange { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
        [field: SerializeField] public float3 ShotPoint { get; private set; }
        [SerializeField] private string _muzzleEffectId;
        [SerializeField] private WeaponPartAttachmentProtocol _towerAttachmentProtocol;
        [SerializeField] private WeaponPartAttachmentProtocol _barrelAttachmentProtocol;

        public float RecommendedRange => math.lerp(MinRange, MaxRange, RecommendedRangePc);
        public abstract bool ContinuousFiring { get; }

        public abstract bool TryGetProjectileId(out string projectileId);
        public abstract bool TryGetRayEffectId(out string rayEffectId);

        public bool TryGetMuzzleEffectId(out string muzzleEffectId)
        {
            if (string.IsNullOrEmpty(_muzzleEffectId))
            {
                muzzleEffectId = default;
                return false;
            }
            else
            {
                muzzleEffectId = _muzzleEffectId;
                return true;
            }
        }

        public bool TryGetTowerAttachmentProtocol(out WeaponPartAttachmentProtocol protocol)
        {
            protocol = _towerAttachmentProtocol;
            return protocol.IsValid;
        }

        public bool TryGetBarrelAttachmentProtocol(out WeaponPartAttachmentProtocol protocol)
        {
            protocol = _barrelAttachmentProtocol;
            return protocol.IsValid;
        }
    }
}
