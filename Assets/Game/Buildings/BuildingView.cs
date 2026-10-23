using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingView : DisposableGameObject, IView, IBuildingView, IMultiColliderView
    {
        [Serializable]
        public struct SerializedBlockZoneConfig
        {
            public bool IsValid;
            public BlockZoneMode Mode;
            public int ExtentA;
            public int ExtentB;
            public CellOccupationLevelFlags Flags;
        }

        [SerializeField] private DamageableEntityParameters _parameters;
        [SerializeField] private Collider[] _colliders;
        [SerializeField] private string _destroyEffectKey;
        [SerializeField] private BuildingFunctionFlags _functionFlags;
        [SerializeField] private SerializedBlockZoneConfig _blockConfig;
        public string ViewDestroyEffectKey => _destroyEffectKey;
        public BuildingFunctionFlags FunctionFlags => _functionFlags;

        public void FillCollidersList(ICollection<int> colliderInstanceIds)
        {
            foreach (var collider in _colliders)
            {
                colliderInstanceIds.Add(collider.GetInstanceID());
            }
        }

        public DamageableEntityParameters GetParameters() => _parameters;
        public IView GetView() => this;

        public bool TryGetBlockZoneConfig(out BlockZoneConfigComponent blockZoneConfigComponent)
        {
            if (!_blockConfig.IsValid)
            {
                blockZoneConfigComponent = default;
                return false;
            }

            blockZoneConfigComponent = _blockConfig.Mode.DoesRequireTwoExtents()
           ? new(_blockConfig.Mode, _blockConfig.ExtentA, _blockConfig.ExtentB, _blockConfig.Flags)
           : new(_blockConfig.Mode, _blockConfig.ExtentA, _blockConfig.Flags);
            return true;
        }
    }
}
