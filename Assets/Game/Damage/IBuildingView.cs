using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public interface IBuildingView : IMonoView
    {
        BuildingFunctionFlags FunctionFlags { get; }

        bool TryGetBlockZoneConfig(out BlockZoneConfigComponent blockZoneConfigComponent);
    }
}
