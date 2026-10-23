using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface IBuildingView : IMonoView
    {
        string ViewDestroyEffectKey { get; }
        BuildingFunctionFlags FunctionFlags { get; }
        DamageableEntityParameters GetParameters();

        bool TryGetBlockZoneConfig(out BlockZoneConfigComponent blockZoneConfigComponent);
    }
}
