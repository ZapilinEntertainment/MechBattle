using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public interface IBuildingFeatureApplier
    {
        BuildingFunctionFlags RequiredFlagsMask { get; }
        void ApplyBuildingFeature(Entity buildingEntity, IBuildingView view);
    
    }
}
