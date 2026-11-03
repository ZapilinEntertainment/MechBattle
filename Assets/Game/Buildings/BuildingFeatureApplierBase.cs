using Scellecs.Morpeh;
using UnityEngine;

namespace ZE.MechBattle
{
    public abstract class BuildingFeatureApplierBase<T> : IBuildingFeatureApplier
        where T : MonoBehaviour
    {
        public abstract BuildingFunctionFlags RequiredFlagsMask { get; }

        public void ApplyBuildingFeature(Entity buildingEntity, IBuildingView view)
        {
            if (view.GameObject.TryGetComponent(out T monoComponent))
                ApplyBuildingFeature(buildingEntity, monoComponent);
        }

        protected abstract void ApplyBuildingFeature(Entity entity, T monoComponent);
    }
}
