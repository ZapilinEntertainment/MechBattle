using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class BuildingView : DisposableGameObject, IView, IBuildingView, IMultiColliderView, IViewContainer
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

        [SerializeField] private Collider[] _colliders;
        [SerializeField] private BuildingFunctionFlags _functionFlags;
        [SerializeField] private SerializedBlockZoneConfig _blockConfig;
        public BuildingFunctionFlags FunctionFlags => _functionFlags;
        public IView ContainedView => this;

        public void FillCollidersList(ICollection<int> colliderInstanceIds)
        {
            foreach (var collider in _colliders)
            {
                colliderInstanceIds.Add(collider.GetInstanceID());
            }
        }

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

        public void OnViewInstanced(IView prefab)
        {
            // todo: here can be replacing simple primitive view ( prefab model) to more detalized one
        }
    }
}
