using System;
using System.Collections.Generic;
using UnityEngine;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class BuildingView : ComplexMonoView, IView, IBuildingView, IMultiColliderView, IViewContainer, IComplexView
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

        public ViewMode ViewMode => ViewMode.InstantMono;

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

        public bool TryGetComplexView(out IComplexView complexView)
        {
            complexView = this;
            return true;
        }

        public override bool TryGetPartView(ViewPartKey key, out IMonoView viewPart)
        {
            if (TryGetComponent<BuildingViewPartsComponent>(out var viewpartsComponent)
               && viewpartsComponent.TryGetViewPart(key, out viewPart))
            {
                return true;
            }

            viewPart = default;
            return false;
        }
    }
}
