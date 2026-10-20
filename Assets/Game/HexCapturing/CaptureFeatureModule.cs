using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CaptureFeatureModule : EcsFeatureModule<CaptureSystemsInstallQueue>
    {
        protected override CaptureSystemsInstallQueue CreateQueue() => new();
    }
}
